using Auth.DTO.Auth;
using Common.DTO;

namespace Auth.Services.Interfaces;

public interface IAuthService
{
    Task<DataResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<DataResponse<MeResponse>> GetMeAsync(Guid userId);
    Task<BaseResponse> RegisterAsync(RegisterRequest request);
}
