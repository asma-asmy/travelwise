using TravelWise.Application.Features.Auth.Dtos;

namespace TravelWise.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<MeResponseDto?> GetMeAsync(Guid userId, CancellationToken ct = default);
}