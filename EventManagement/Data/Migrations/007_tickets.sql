-- EventManagement: persist owned tickets generated from paid checkouts.
-- Run this against event_management_db if upgrading an existing database.

CREATE TABLE IF NOT EXISTS tickets (
    id              UUID PRIMARY KEY,
    checkout_id     UUID NOT NULL REFERENCES checkout_sessions(id) ON DELETE CASCADE,
    user_id         UUID NOT NULL,
    event_id        UUID NOT NULL REFERENCES events(id),
    ticket_type_id  UUID NOT NULL REFERENCES ticket_types(id),
    serial_no       INT NOT NULL,
    qr_payload      TEXT NOT NULL,
    status          SMALLINT NOT NULL DEFAULT 1,
    purchased_at    TIMESTAMP NOT NULL,
    expires_at      TIMESTAMP,
    used_at         TIMESTAMP,
    created_on      TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by      VARCHAR(255) NOT NULL DEFAULT 'system',
    updated_on      TIMESTAMP,
    updated_by      VARCHAR(255)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_tickets_checkout_tickettype_serial
    ON tickets(checkout_id, ticket_type_id, serial_no);

CREATE INDEX IF NOT EXISTS idx_tickets_user_id
    ON tickets(user_id);

CREATE INDEX IF NOT EXISTS idx_tickets_event_id_status
    ON tickets(event_id, status);