using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Common.Interfaces;
using TravelWise.Application.Common.Options;
using TravelWise.Application.Features.Auth;
using TravelWise.Application.Features.Auth.Dtos;
using TravelWise.Domain.Entities;
using TravelWise.Domain.Enums;
using TravelWise.Infrastructure.Persistence;

namespace TravelWise.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly TravelWiseDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly JwtOptions _options;

    public AuthService(
        TravelWiseDbContext db,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IOptions<JwtOptions> options)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _options = options.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("An account with this email already exists.");

        var travellerRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.Name == UserRoleType.Traveller, ct)
            ?? throw new AppException("Traveller role is not configured.", 500);

        var user = new User
        {
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = travellerRole.Id });
        await _db.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, new[] { travellerRole.Name.ToString() }, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenException("This account is disabled.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name.ToString()).ToList();
        return await IssueTokensAsync(user, roles, ct);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken ct = default)
    {
        var existing = await _db.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (existing is null || !existing.IsActive)
            throw new UnauthorizedException("Refresh token is invalid or expired.");

        // Revoke the old token
        existing.RevokedAt = DateTime.UtcNow;

        var roles = existing.User.UserRoles.Select(ur => ur.Role.Name.ToString()).ToList();
        var response = await IssueTokensAsync(existing.User, roles, ct);

        // Link new token
        existing.ReplacedByToken = response.RefreshToken;
        await _db.SaveChangesAsync(ct);

        return response;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);
        if (token is null || token.RevokedAt is not null) return;

        token.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<MeResponseDto?> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;

        return new MeResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Roles = user.UserRoles.Select(ur => ur.Role.Name.ToString()).ToList()
        };
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, IEnumerable<string> roles, CancellationToken ct)
    {
        var roleList = roles.ToList();
        var (accessToken, accessExpiresAt) = _jwt.CreateAccessToken(user, roleList);
        var refreshTokenString = _jwt.CreateRefreshTokenString();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = refreshExpiresAt
        });
        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessExpiresAt,
            RefreshToken = refreshTokenString,
            RefreshTokenExpiresAtUtc = refreshExpiresAt,
            Roles = roleList
        };
    }
}