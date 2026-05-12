using Common.DTO;
using Common.Exceptions;
using EventManagement.DTO.Checkout;
using EventManagement.DTO.Event;
using EventManagement.Enums;
using EventManagement.Infrastructures;
using EventManagement.Model;
using EventManagement.Repositories.Interfaces;
using EventManagement.Services.Interfaces;
using Microsoft.Extensions.Options;
using QRCoder;

namespace EventManagement.Services;

public class CheckoutService : ICheckoutService
{
    private readonly CheckoutOptions _checkoutOptions;
    private readonly ICheckoutLockService _checkoutLockService;
    private readonly ICheckoutRepository _checkoutRepository;
    private readonly IEmailService _emailService;
    private readonly IEventRepository _eventRepository;
    private readonly FrontendOptions _frontendOptions;
    private readonly ILogger<CheckoutService> _logger;
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IXenditClient _xenditClient;
    private readonly XenditOptions _xenditOptions;

    public CheckoutService(
        IEventRepository eventRepository,
        ITicketTypeRepository ticketTypeRepository,
        ITicketRepository ticketRepository,
        ICheckoutRepository checkoutRepository,
        IEmailService emailService,
        ICheckoutLockService checkoutLockService,
        IXenditClient xenditClient,
        IOptions<CheckoutOptions> checkoutOptions,
        IOptions<XenditOptions> xenditOptions,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<CheckoutService> logger
    )
    {
        _eventRepository = eventRepository;
        _ticketTypeRepository = ticketTypeRepository;
        _ticketRepository = ticketRepository;
        _checkoutRepository = checkoutRepository;
        _emailService = emailService;
        _checkoutLockService = checkoutLockService;
        _xenditClient = xenditClient;
        _checkoutOptions = checkoutOptions.Value;
        _xenditOptions = xenditOptions.Value;
        _frontendOptions = frontendOptions.Value;
        _logger = logger;
    }

    public async Task<DataResponse<CheckoutResponse>> CreateAsync(
        CreateCheckoutRequest request,
        CheckoutUserContext userContext,
        CancellationToken cancellationToken
    )
    {
        if (request.EventId == Guid.Empty)
            throw new BadRequestException("EventId is required.");

        if (request.Items.Count == 0)
            throw new BadRequestException("At least one checkout item is required.");

        var normalizedItems = request
            .Items.GroupBy(item => item.TicketTypeId)
            .Select(group => new CreateCheckoutItemRequest
            {
                TicketTypeId = group.Key,
                Quantity = group.Sum(item => item.Quantity),
            })
            .ToList();

        if (normalizedItems.Any(item => item.TicketTypeId == Guid.Empty || item.Quantity <= 0))
            throw new BadRequestException(
                "Each checkout item must include a valid ticketTypeId and quantity."
            );

        var ev = await _eventRepository.GetByIdAsync(request.EventId);
        if (ev is null)
            throw new NotFoundException("Event not found.");

        ValidateEventAvailability(ev.Status);

        var eventTicketTypes = await _ticketTypeRepository.GetByEventIdAsync(request.EventId);
        var ticketTypeMap = eventTicketTypes.List.ToDictionary(ticket => ticket.Id);

        var requestedTicketTypes = new List<(TicketTypeResponse TicketType, int Quantity)>();
        foreach (var item in normalizedItems)
        {
            if (!ticketTypeMap.TryGetValue(item.TicketTypeId, out var ticketType))
                throw new NotFoundException(
                    "One or more ticket types do not belong to the selected event."
                );

            ValidateTicketAvailability(ticketType, item.Quantity);
            requestedTicketTypes.Add((ticketType, item.Quantity));
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(Math.Max(_checkoutOptions.HoldDurationMinutes, 1));
        var checkoutId = Guid.NewGuid();
        var externalId = $"checkout-{checkoutId:N}";
        var reservedItems = new List<(Guid TicketTypeId, int Quantity)>();

        foreach (var requestedItem in requestedTicketTypes)
        {
            var activeReservedQuantity = await _checkoutRepository.GetActiveReservedQuantityAsync(
                requestedItem.TicketType.Id,
                now
            );

            await _checkoutLockService.EnsureTicketStockAsync(
                requestedItem.TicketType.Id,
                requestedItem.TicketType.AvailableSeats,
                activeReservedQuantity
            );

            var reservation = await _checkoutLockService.ReserveAsync(
                checkoutId,
                requestedItem.TicketType.Id,
                requestedItem.Quantity,
                expiresAt
            );

            if (!reservation.IsReserved)
            {
                foreach (var reservedItem in reservedItems)
                {
                    await _checkoutLockService.ReleaseReservationAsync(
                        checkoutId,
                        reservedItem.TicketTypeId,
                        reservedItem.Quantity
                    );
                }

                var message =
                    reservation.RemainingStock <= 0
                        ? $"Ticket '{requestedItem.TicketType.Name}' is no longer available."
                        : $"Only {reservation.RemainingStock} ticket(s) are available right now for '{requestedItem.TicketType.Name}'.";
                throw new ConflictException(message);
            }

            reservedItems.Add((requestedItem.TicketType.Id, requestedItem.Quantity));
        }

        var cartCurrency = requestedTicketTypes
            .Select(item =>
                string.IsNullOrWhiteSpace(item.TicketType.Currency)
                    ? _xenditOptions.Currency
                    : item.TicketType.Currency
            )
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cartCurrency.Count != 1)
        {
            foreach (var reservedItem in reservedItems)
            {
                await _checkoutLockService.ReleaseReservationAsync(
                    checkoutId,
                    reservedItem.TicketTypeId,
                    reservedItem.Quantity
                );
            }

            throw new BadRequestException("All checkout items must use the same currency.");
        }

        var checkoutItems = requestedTicketTypes
            .Select(item => new CheckoutSessionItem
            {
                Id = Guid.NewGuid(),
                CheckoutId = checkoutId,
                TicketTypeId = item.TicketType.Id,
                TicketName = item.TicketType.Name,
                Quantity = item.Quantity,
                UnitPrice = item.TicketType.Price,
                LineTotal = item.TicketType.Price * item.Quantity,
                Currency = string.IsNullOrWhiteSpace(item.TicketType.Currency)
                    ? _xenditOptions.Currency
                    : item.TicketType.Currency,
                CreatedOn = now,
                CreatedBy = userContext.Email,
            })
            .ToList();

        var checkoutSession = new CheckoutSession
        {
            Id = checkoutId,
            InvoiceExternalId = externalId,
            UserId = userContext.UserId,
            UserEmail = userContext.Email,
            UserFullName = userContext.FullName,
            EventId = ev.Id,
            Quantity = checkoutItems.Sum(item => item.Quantity),
            TotalAmount = checkoutItems.Sum(item => item.LineTotal),
            Currency = cartCurrency[0],
            Status = CheckoutStatusEnum.Pending,
            ExpiresAt = expiresAt,
            CreatedOn = now,
            CreatedBy = userContext.Email,
        };

        try
        {
            var created = await _checkoutRepository.CreateAsync(checkoutSession, checkoutItems);
            if (created <= 0)
                throw new InternalServerException("Failed to create checkout session.");

            var invoiceDescription =
                $"{ev.Title} - {checkoutItems.Count} item(s), {checkoutSession.Quantity} ticket(s)";

            var invoice = await _xenditClient.CreateInvoiceAsync(
                new XenditCreateInvoiceRequest
                {
                    ExternalId = externalId,
                    Amount = checkoutSession.TotalAmount,
                    Currency = checkoutSession.Currency,
                    PayerEmail = userContext.Email,
                    Description = invoiceDescription,
                    InvoiceDurationSeconds = Math.Max(
                        _xenditOptions.InvoiceDurationSeconds,
                        _checkoutOptions.HoldDurationMinutes * 60
                    ),
                    SuccessRedirectUrl = request.SuccessRedirectUrl,
                    FailureRedirectUrl = request.FailureRedirectUrl,
                },
                cancellationToken
            );

            var invoiceRows = await _checkoutRepository.AttachInvoiceAsync(
                checkoutId,
                invoice.Id,
                invoice.InvoiceUrl,
                DateTime.UtcNow,
                "xendit"
            );

            if (invoiceRows <= 0)
                throw new InternalServerException("Failed to attach invoice to checkout session.");

            checkoutSession.XenditInvoiceId = invoice.Id;
            checkoutSession.XenditInvoiceUrl = invoice.InvoiceUrl;

            await SendOrderCreatedEmailAsync(checkoutSession, ev.Title, checkoutItems);

            return new DataResponse<CheckoutResponse>
            {
                IsOk = true,
                Message = "Checkout created successfully.",
                Data = MapResponse(checkoutSession, ev.Title, checkoutItems),
            };
        }
        catch
        {
            await _checkoutRepository.MarkFailedAsync(
                checkoutId,
                "Failed to create payment invoice.",
                DateTime.UtcNow,
                "system"
            );

            foreach (var reservedItem in reservedItems)
            {
                await _checkoutLockService.ReleaseReservationAsync(
                    checkoutId,
                    reservedItem.TicketTypeId,
                    reservedItem.Quantity
                );
            }

            throw;
        }
    }

    public async Task<DataResponse<CheckoutResponse>> GetByIdAsync(Guid checkoutId, Guid userId)
    {
        var checkout = await _checkoutRepository.GetByIdAsync(checkoutId);
        if (checkout is null)
            throw new NotFoundException("Checkout session not found.");

        if (checkout.UserId != userId)
            throw new ForbiddenException("You cannot access this checkout session.");

        checkout = await EnsureExpiredIfNeededAsync(checkout);
        var checkoutItems = await _checkoutRepository.GetItemsByCheckoutIdAsync(checkout.Id);

        var ev = await _eventRepository.GetByIdAsync(checkout.EventId);

        return new DataResponse<CheckoutResponse>
        {
            IsOk = true,
            Data = MapResponse(checkout, ev?.Title ?? "Event", checkoutItems),
        };
    }

    public async Task<DataResponse<AttendeeDashboardResponse>> GetAttendeeDashboardAsync(
        Guid userId
    )
    {
        var orders = await GetMyOrdersAsync(userId);
        var ownedTicketsResponse = await GetMyOwnedTicketsAsync(userId);
        var ownedTickets = ownedTicketsResponse.Data ?? [];

        var activeTickets = ownedTickets
            .Where(ticket =>
                string.Equals(ticket.Status, "active", StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        var now = DateTime.UtcNow;
        var upcomingEvents = activeTickets
            .Where(ticket => ticket.Event is not null && ticket.Event.EndDate >= now)
            .Select(ticket => ticket.Event!)
            .GroupBy(ev => ev.Id)
            .Select(group => group.First())
            .OrderBy(ev => ev.StartDate)
            .Take(6)
            .ToList();

        var recentTickets = ownedTickets
            .OrderByDescending(ticket => ticket.PurchasedAt)
            .Take(4)
            .ToList();

        return new DataResponse<AttendeeDashboardResponse>
        {
            IsOk = true,
            Data = new AttendeeDashboardResponse
            {
                Summary = new AttendeeDashboardSummaryResponse
                {
                    ActiveTickets = activeTickets.Count,
                    TotalOrders = orders.Data?.Count ?? 0,
                    UpcomingEvents = upcomingEvents.Count,
                },
                RecentTickets = recentTickets,
                UpcomingEvents = upcomingEvents,
            },
        };
    }

    public async Task<DataResponse<List<OrderListItemResponse>>> GetMyOrdersAsync(Guid userId)
    {
        var checkouts = await _checkoutRepository.GetByUserIdAsync(userId);
        if (checkouts.Count == 0)
        {
            return new DataResponse<List<OrderListItemResponse>> { IsOk = true, Data = [] };
        }

        var normalizedCheckouts = new List<CheckoutSession>(checkouts.Count);
        var eventIds = new HashSet<Guid>();

        foreach (var checkout in checkouts)
        {
            var updatedCheckout = await EnsureExpiredIfNeededAsync(checkout);
            normalizedCheckouts.Add(updatedCheckout);
            eventIds.Add(updatedCheckout.EventId);
        }

        var eventById = new Dictionary<Guid, EventResponse>();
        foreach (var eventId in eventIds)
        {
            var ev = await _eventRepository.GetByIdAsync(eventId);
            if (ev is not null)
                eventById[eventId] = ev;
        }

        var orders = new List<OrderListItemResponse>(normalizedCheckouts.Count);

        foreach (var checkout in normalizedCheckouts)
        {
            var checkoutItems = await _checkoutRepository.GetItemsByCheckoutIdAsync(checkout.Id);
            orders.Add(
                new OrderListItemResponse
                {
                    Id = $"ord-{checkout.Id:N}",
                    CheckoutId = checkout.Id,
                    ExternalId = checkout.InvoiceExternalId,
                    UserId = userId.ToString("N"),
                    Status = checkout.Status.ToString().ToLowerInvariant(),
                    Event = eventById.GetValueOrDefault(checkout.EventId),
                    InvoiceUrl = checkout.XenditInvoiceUrl ?? string.Empty,
                    PaymentMethod = checkout.PaymentMethod,
                    TotalQuantity = checkout.Quantity,
                    TotalAmount = checkout.TotalAmount,
                    Currency = checkout.Currency,
                    CreatedAt = checkout.CreatedOn,
                    ExpiresAt = checkout.ExpiresAt,
                    PaidAt = checkout.PaidAt,
                    FailureReason = checkout.FailureReason,
                    Items = checkoutItems
                        .Select(item => new CheckoutItemResponse
                        {
                            TicketTypeId = item.TicketTypeId,
                            TicketName = item.TicketName,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            LineTotal = item.LineTotal,
                            Currency = item.Currency,
                        })
                        .ToList(),
                }
            );
        }

        return new DataResponse<List<OrderListItemResponse>> { IsOk = true, Data = orders };
    }

    public async Task<DataResponse<OrderListItemResponse>> GetMyOrderByIdAsync(
        Guid userId,
        string orderId
    )
    {
        var checkoutId = ParseOrderId(orderId);
        var checkout = await _checkoutRepository.GetByIdAsync(checkoutId);
        if (checkout is null || checkout.UserId != userId)
            throw new NotFoundException("Order not found.");

        var normalizedCheckout = await EnsureExpiredIfNeededAsync(checkout);
        var ev = await _eventRepository.GetByIdAsync(normalizedCheckout.EventId);
        var checkoutItems = await _checkoutRepository.GetItemsByCheckoutIdAsync(
            normalizedCheckout.Id
        );

        return new DataResponse<OrderListItemResponse>
        {
            IsOk = true,
            Data = MapOrderResponse(normalizedCheckout, userId, ev, checkoutItems),
        };
    }

    public async Task<DataResponse<List<OwnedTicketResponse>>> GetMyOwnedTicketsAsync(Guid userId)
    {
        var persistedTickets = await _ticketRepository.GetByUserIdAsync(userId);
        if (persistedTickets.Count == 0)
        {
            return new DataResponse<List<OwnedTicketResponse>> { IsOk = true, Data = [] };
        }

        var eventIds = persistedTickets.Select(ticket => ticket.EventId).Distinct().ToList();

        var eventById = new Dictionary<Guid, EventResponse>();
        foreach (var eventId in eventIds)
        {
            var ev = await _eventRepository.GetByIdAsync(eventId);
            if (ev is not null)
                eventById[eventId] = ev;
        }

        var ticketTypeIds = persistedTickets
            .Select(ticket => ticket.TicketTypeId)
            .Distinct()
            .ToHashSet();
        var ticketTypeById = new Dictionary<Guid, TicketTypeResponse>(ticketTypeIds.Count);
        var ticketTypesByEventId = await _ticketTypeRepository.GetByEventIdsAsync(eventIds);
        foreach (var ticketTypeList in ticketTypesByEventId.Values)
        {
            foreach (
                var ticketType in ticketTypeList.Where(ticketType =>
                    ticketTypeIds.Contains(ticketType.Id)
                )
            )
            {
                ticketTypeById[ticketType.Id] = ticketType;
            }
        }

        var now = DateTime.UtcNow;
        var ownedTickets = new List<OwnedTicketResponse>(persistedTickets.Count);

        foreach (var ticket in persistedTickets)
        {
            var eventEndDate = eventById.GetValueOrDefault(ticket.EventId)?.EndDate;
            var status = ResolveTicketStatus(ticket, now, eventEndDate);
            if (
                status == TicketOwnershipStatusEnum.Expired
                && ticket.Status == TicketOwnershipStatusEnum.Active
            )
            {
                await _ticketRepository.MarkAsExpiredAsync(ticket.Id, now, "system");
            }

            ownedTickets.Add(
                new OwnedTicketResponse
                {
                    Id = ticket.Id.ToString("N"),
                    TicketTypeId = ticket.TicketTypeId.ToString("N"),
                    TicketType = ticketTypeById.GetValueOrDefault(ticket.TicketTypeId),
                    UserId = ticket.UserId.ToString("N"),
                    OrderId = $"ord-{ticket.CheckoutId:N}",
                    Event = eventById.GetValueOrDefault(ticket.EventId),
                    QrCode = ticket.QrPayload,
                    Status = status.ToString().ToLowerInvariant(),
                    PurchasedAt = ticket.PurchasedAt,
                }
            );
        }

        return new DataResponse<List<OwnedTicketResponse>> { IsOk = true, Data = ownedTickets };
    }

    public async Task<DataResponse<List<OwnedTicketResponse>>> GetMyOwnedTicketsByEventAsync(
        Guid userId,
        Guid eventId
    )
    {
        var allTickets = await GetMyOwnedTicketsAsync(userId);
        return new DataResponse<List<OwnedTicketResponse>>
        {
            IsOk = true,
            Data =
                allTickets
                    .Data?.Where(ticket => ticket.Event is not null && ticket.Event.Id == eventId)
                    .ToList()
                ?? [],
        };
    }

    public async Task<BaseResponse> MarkTicketAsUsedAsync(Guid ticketId, Guid organizerId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket is null)
            throw new NotFoundException("Ticket not found.");

        var ev = await _eventRepository.GetByIdAsync(ticket.EventId);
        if (ev is null)
            throw new NotFoundException("Ticket event not found.");

        if (ev.OrganizerId != organizerId)
            throw new ForbiddenException("You cannot update a ticket for this event.");

        var now = DateTime.UtcNow;
        var status = ResolveTicketStatus(ticket, now, ev.EndDate);

        if (status == TicketOwnershipStatusEnum.Expired)
        {
            if (ticket.Status == TicketOwnershipStatusEnum.Active)
                await _ticketRepository.MarkAsExpiredAsync(ticket.Id, now, "system");

            throw new ConflictException("Ticket is expired and cannot be used.");
        }

        if (status == TicketOwnershipStatusEnum.Used)
        {
            return new BaseResponse
            {
                IsOk = true,
                Message = "Ticket is already marked as used.",
                AnyChange = 0,
            };
        }

        if (status != TicketOwnershipStatusEnum.Active)
            throw new ConflictException($"Ticket cannot be used from status '{status}'.");

        var rows = await _ticketRepository.MarkAsUsedAsync(
            ticket.Id,
            now,
            $"organizer:{organizerId:N}"
        );

        return new BaseResponse
        {
            IsOk = true,
            Message = rows > 0 ? "Ticket marked as used." : "Ticket state did not change.",
            AnyChange = rows,
        };
    }

    public async Task<string> GetInvoiceUrlAsync(Guid checkoutId, Guid userId)
    {
        var response = await GetByIdAsync(checkoutId, userId);
        if (response.Data is null || string.IsNullOrWhiteSpace(response.Data.InvoiceUrl))
            throw new NotFoundException("Invoice URL is not available for this checkout session.");

        return response.Data.InvoiceUrl;
    }

    public async Task<BaseResponse> HandleWebhookAsync(
        XenditInvoiceWebhookRequest request,
        string? callbackToken
    )
    {
        if (string.IsNullOrWhiteSpace(_xenditOptions.WebhookToken))
            throw new InternalServerException("Xendit WebhookToken is not configured.");

        if (!string.Equals(callbackToken, _xenditOptions.WebhookToken, StringComparison.Ordinal))
            throw new UnauthorizedException("Invalid Xendit callback token.");

        if (string.IsNullOrWhiteSpace(request.ExternalId))
            throw new BadRequestException("Webhook payload does not include external_id.");

        var checkout = await _checkoutRepository.GetByInvoiceExternalIdAsync(request.ExternalId);
        if (checkout is null)
        {
            return new BaseResponse
            {
                IsOk = true,
                Message = "Webhook ignored. Checkout session not found.",
            };
        }

        var now = DateTime.UtcNow;
        var status = request.Status.ToUpperInvariant();

        if (status is "PAID" or "SETTLED")
        {
            var checkoutItems = await _checkoutRepository.GetItemsByCheckoutIdAsync(checkout.Id);
            var paidRows = await _checkoutRepository.MarkPaidAsync(
                checkout.Id,
                request.Id,
                request.PaymentMethod,
                request.PaidAt ?? now,
                now,
                "xendit-webhook"
            );

            if (paidRows > 0)
            {
                foreach (var checkoutItem in checkoutItems)
                    await _checkoutLockService.FinalizeReservationAsync(
                        checkout.Id,
                        checkoutItem.TicketTypeId
                    );

                var refreshedCheckout = await _checkoutRepository.GetByIdAsync(checkout.Id);
                var ev = await _eventRepository.GetByIdAsync(checkout.EventId);
                if (refreshedCheckout is not null)
                    await SendOrderPaidEmailAsync(
                        refreshedCheckout,
                        ev?.Title ?? "Event",
                        checkoutItems
                    );
            }

            return new BaseResponse
            {
                IsOk = true,
                Message = paidRows > 0 ? "Checkout marked as paid." : "Checkout already finalized.",
                AnyChange = paidRows,
            };
        }

        if (status == "EXPIRED")
        {
            return await ExpireCheckoutAsync(
                checkout,
                request.FailureReason ?? "Checkout expired before payment.",
                "xendit-webhook"
            );
        }

        if (status is "FAILED" or "CANCELLED")
        {
            var checkoutItems = await _checkoutRepository.GetItemsByCheckoutIdAsync(checkout.Id);
            var failedRows = await _checkoutRepository.MarkFailedAsync(
                checkout.Id,
                request.FailureReason ?? "Payment failed.",
                now,
                "xendit-webhook"
            );

            if (failedRows > 0)
            {
                foreach (var checkoutItem in checkoutItems)
                    await _checkoutLockService.ReleaseReservationAsync(
                        checkout.Id,
                        checkoutItem.TicketTypeId,
                        checkoutItem.Quantity
                    );
            }

            return new BaseResponse
            {
                IsOk = true,
                Message =
                    failedRows > 0 ? "Checkout marked as failed." : "Checkout already finalized.",
                AnyChange = failedRows,
            };
        }

        return new BaseResponse
        {
            IsOk = true,
            Message = $"Webhook status '{request.Status}' ignored.",
        };
    }

    public async Task ProcessExpiredCheckoutsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var expiredCheckouts = await _checkoutRepository.GetExpiredPendingAsync(
            DateTime.UtcNow,
            Math.Max(_checkoutOptions.ExpirationSweepBatchSize, 1)
        );

        foreach (var checkout in expiredCheckouts)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ExpireCheckoutAsync(checkout, "Checkout expired before payment.", "sweeper");
        }
    }

    private async Task<CheckoutSession> EnsureExpiredIfNeededAsync(CheckoutSession checkout)
    {
        if (checkout.Status != CheckoutStatusEnum.Pending || checkout.ExpiresAt > DateTime.UtcNow)
            return checkout;

        await ExpireCheckoutAsync(checkout, "Checkout expired before payment.", "status-check");
        return (await _checkoutRepository.GetByIdAsync(checkout.Id)) ?? checkout;
    }

    private async Task<BaseResponse> ExpireCheckoutAsync(
        CheckoutSession checkout,
        string reason,
        string actor
    )
    {
        var checkoutItems = await _checkoutRepository.GetItemsByCheckoutIdAsync(checkout.Id);
        var expiredRows = await _checkoutRepository.MarkExpiredAsync(
            checkout.Id,
            reason,
            DateTime.UtcNow,
            actor
        );

        if (expiredRows > 0)
        {
            foreach (var checkoutItem in checkoutItems)
                await _checkoutLockService.ReleaseReservationAsync(
                    checkout.Id,
                    checkoutItem.TicketTypeId,
                    checkoutItem.Quantity
                );
        }

        return new BaseResponse
        {
            IsOk = true,
            Message = expiredRows > 0 ? "Checkout expired." : "Checkout already finalized.",
            AnyChange = expiredRows,
        };
    }

    private OrderListItemResponse MapOrderResponse(
        CheckoutSession checkout,
        Guid userId,
        EventResponse? ev,
        IReadOnlyCollection<CheckoutSessionItem> checkoutItems
    ) =>
        new()
        {
            Id = $"ord-{checkout.Id:N}",
            CheckoutId = checkout.Id,
            ExternalId = checkout.InvoiceExternalId,
            UserId = userId.ToString("N"),
            Status = checkout.Status.ToString().ToLowerInvariant(),
            Event = ev,
            InvoiceUrl = checkout.XenditInvoiceUrl ?? string.Empty,
            PaymentMethod = checkout.PaymentMethod,
            TotalQuantity = checkout.Quantity,
            TotalAmount = checkout.TotalAmount,
            Currency = checkout.Currency,
            CreatedAt = checkout.CreatedOn,
            ExpiresAt = checkout.ExpiresAt,
            PaidAt = checkout.PaidAt,
            FailureReason = checkout.FailureReason,
            Items = checkoutItems
                .Select(item => new CheckoutItemResponse
                {
                    TicketTypeId = item.TicketTypeId,
                    TicketName = item.TicketName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal,
                    Currency = item.Currency,
                })
                .ToList(),
        };

    private static Guid ParseOrderId(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new NotFoundException("Order not found.");

        var normalized = orderId.StartsWith("ord-", StringComparison.OrdinalIgnoreCase)
            ? orderId[4..]
            : orderId;

        if (!Guid.TryParse(normalized, out var checkoutId))
            throw new NotFoundException("Order not found.");

        return checkoutId;
    }

    private async Task SendOrderCreatedEmailAsync(
        CheckoutSession checkout,
        string eventTitle,
        IReadOnlyCollection<CheckoutSessionItem> items
    )
    {
        try
        {
            var orderId = $"ord-{checkout.Id:N}";
            var orderDetailUrl = BuildFrontendUrl($"/dashboard/orders/{orderId}");
            var lines = string.Join(
                string.Empty,
                items.Select(item =>
                    $"<li>{item.TicketName} x{item.Quantity} - {item.LineTotal:N0} {item.Currency}</li>"
                )
            );

            var body = $"""
                <p>Hi {checkout.UserFullName},</p>
                <p>Your order for <strong>{eventTitle}</strong> has been created.</p>
                <p><strong>Order ID:</strong> {orderId}<br/>
                <strong>Total:</strong> {checkout.TotalAmount:N0} {checkout.Currency}<br/>
                <strong>Expires At:</strong> {checkout.ExpiresAt:yyyy-MM-dd HH:mm} UTC</p>
                <p><strong>Items</strong></p>
                <ul>{lines}</ul>
                <p>
                    <a href=\"{checkout.XenditInvoiceUrl}\">Complete Payment</a><br/>
                    <a href=\"{orderDetailUrl}\">View Order Detail</a>
                </p>
                <p>Thanks,<br/>GetTicket</p>
                """;

            await _emailService.SendAsync(
                checkout.UserEmail,
                $"[GetTicket] Order Created - {orderId}",
                body
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed sending order-created email for checkout {CheckoutId}",
                checkout.Id
            );
        }
    }

    private async Task SendOrderPaidEmailAsync(
        CheckoutSession checkout,
        string eventTitle,
        IReadOnlyCollection<CheckoutSessionItem> items
    )
    {
        try
        {
            var orderId = $"ord-{checkout.Id:N}";
            var orderDetailUrl = BuildFrontendUrl($"/dashboard/orders/{orderId}");
            var ticketListUrl = BuildFrontendUrl($"/dashboard/tickets/{checkout.EventId}");
            var lines = string.Join(
                string.Empty,
                items.Select(item =>
                    $"<li>{item.TicketName} x{item.Quantity} - {item.LineTotal:N0} {item.Currency}</li>"
                )
            );

            var attachments = await BuildQrAttachmentsAsync(checkout.Id);
            var body = $"""
                <p>Hi {checkout.UserFullName},</p>
                <p>Your payment for <strong>{eventTitle}</strong> is confirmed.</p>
                <p><strong>Order ID:</strong> {orderId}<br/>
                <strong>Total Paid:</strong> {checkout.TotalAmount:N0} {checkout.Currency}<br/>
                <strong>Paid At:</strong> {checkout.PaidAt:yyyy-MM-dd HH:mm} UTC</p>
                <p><strong>Items</strong></p>
                <ul>{lines}</ul>
                <p>
                    <a href=\"{ticketListUrl}\">Open Your Ticket List</a><br/>
                    <a href=\"{orderDetailUrl}\">View Order Detail</a>
                </p>
                <p>QR code attachments are included in this email.</p>
                <p>Thanks,<br/>GetTicket</p>
                """;

            await _emailService.SendAsync(
                checkout.UserEmail,
                $"[GetTicket] Payment Confirmed - {orderId}",
                body,
                attachments
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed sending paid-order email for checkout {CheckoutId}",
                checkout.Id
            );
        }
    }

    private async Task<IReadOnlyCollection<EmailAttachment>> BuildQrAttachmentsAsync(
        Guid checkoutId
    )
    {
        var tickets = await _ticketRepository.GetByCheckoutIdAsync(checkoutId);
        if (tickets.Count == 0)
            return [];

        return tickets
            .Take(20)
            .Select(ticket =>
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(
                    ticket.QrPayload,
                    QRCodeGenerator.ECCLevel.Q
                );
                var qrPng = new PngByteQRCode(qrData);
                var bytes = qrPng.GetGraphic(10);

                return new EmailAttachment
                {
                    FileName = $"ticket-{ticket.Id:N}.png",
                    ContentType = "image/png",
                    Content = bytes,
                };
            })
            .ToList();
    }

    private string BuildFrontendUrl(string path)
    {
        var baseUrl = (_frontendOptions.BaseUrl ?? "http://localhost:3000").TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        return $"{baseUrl}{normalizedPath}";
    }

    private static void ValidateEventAvailability(string status)
    {
        if (
            !string.Equals(status, "published", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, "ongoing", StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ConflictException("This event is not available for checkout.");
        }
    }

    private static TicketOwnershipStatusEnum ResolveTicketStatus(
        Ticket ticket,
        DateTime now,
        DateTime? eventEndDate
    )
    {
        var isEndedByEventTime = eventEndDate.HasValue && eventEndDate.Value <= now;
        var isEndedByTicketExpiry = ticket.ExpiresAt.HasValue && ticket.ExpiresAt.Value <= now;

        if (
            ticket.Status == TicketOwnershipStatusEnum.Active
            && (isEndedByEventTime || isEndedByTicketExpiry)
        )
        {
            return TicketOwnershipStatusEnum.Expired;
        }

        return ticket.Status;
    }

    private static void ValidateTicketAvailability(TicketTypeResponse ticketType, int quantity)
    {
        var now = DateTime.UtcNow;

        if (!string.Equals(ticketType.Status, "available", StringComparison.OrdinalIgnoreCase))
            throw new ConflictException("Ticket is not available.");

        if (ticketType.SaleStartDate.HasValue && ticketType.SaleStartDate.Value > now)
            throw new ConflictException("Ticket sale has not started yet.");

        if (ticketType.SaleEndDate.HasValue && ticketType.SaleEndDate.Value <= now)
            throw new ConflictException("Ticket sale has ended.");

        if (ticketType.AvailableSeats <= 0)
            throw new ConflictException("Ticket is no longer available.");

        if (quantity > ticketType.TotalSeats)
            throw new ConflictException("Requested quantity exceeds ticket capacity.");
    }

    private static CheckoutResponse MapResponse(
        CheckoutSession checkout,
        string eventTitle,
        IReadOnlyCollection<CheckoutSessionItem> checkoutItems
    ) =>
        new()
        {
            CheckoutId = checkout.Id,
            ExternalId = checkout.InvoiceExternalId,
            InvoiceUrl = checkout.XenditInvoiceUrl ?? string.Empty,
            PaymentPagePath = $"/api/checkout/{checkout.Id}/pay",
            Status = checkout.Status.ToString().ToLowerInvariant(),
            EventId = checkout.EventId,
            EventTitle = eventTitle,
            TotalQuantity = checkout.Quantity,
            TotalAmount = checkout.TotalAmount,
            Currency = checkout.Currency,
            ExpiresAt = checkout.ExpiresAt,
            PaidAt = checkout.PaidAt,
            FailureReason = checkout.FailureReason,
            Items = checkoutItems
                .Select(item => new CheckoutItemResponse
                {
                    TicketTypeId = item.TicketTypeId,
                    TicketName = item.TicketName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal,
                    Currency = item.Currency,
                })
                .ToList(),
        };
}
