using Auth.DTO.Auth;
using Auth.Infrastructures;
using Auth.Model;
using Auth.Repositories.Interfaces;
using Auth.Services.Interfaces;
using Common.DTO;

namespace Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IUserRepository userRepository, JwtHelper jwtHelper)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<DataResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new DataResponse<LoginResponse>
            {
                IsOk = false,
                ErrorMessage = "Invalid email or password.",
            };
        }

        if (!user.IsActive)
        {
            return new DataResponse<LoginResponse>
            {
                IsOk = false,
                ErrorMessage = "Account is inactive.",
            };
        }

        var token = _jwtHelper.GenerateToken(user);

        return new DataResponse<LoginResponse>
        {
            IsOk = true,
            Message = "Login successful.",
            Data = new LoginResponse
            {
                Token = token,
                FullName = user.FullName,
                Role = user.Role.ToString(),
            },
        };
    }

    public async Task<BaseResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
        {
            return new BaseResponse { IsOk = false, ErrorMessage = "Email is already registered." };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = request.Email,
        };

        var rows = await _userRepository.CreateAsync(user);

        return new BaseResponse
        {
            IsOk = rows > 0,
            Message = rows > 0 ? "Registration successful." : "Registration failed.",
            AnyChange = rows,
        };
    }
}
