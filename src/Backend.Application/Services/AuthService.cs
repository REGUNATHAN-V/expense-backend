using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Application.Mappings;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IAuthRepository authRepository,
        IConfiguration configuration)
    {
        _authRepository = authRepository;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetJwtSettings(
            out var jwtKey,
            out var issuer,
            out var audience,
            out var expiryMinutes))
        {
            return Result<AuthResponseDto>.Failure(
                "JWT configuration is invalid",
                ResultErrorType.Failure);
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        var existingUser = await _authRepository.GetByEmailAsync(
            email, cancellationToken);

        if (existingUser is not null)
        {
            return Result<AuthResponseDto>.Failure(
                "Email already exists",
                ResultErrorType.Conflict);
        }

        var user = new UserEntity
        {
            Name = dto.Name.Trim(),
            Email = email,
            Role = Enum.IsDefined(typeof(UserRole), dto.Role) ? dto.Role : (int)UserRole.Maker, // DEV ONLY: lets register set 1, 2 or 3
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _authRepository.AddAsync(user, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        var token = GenerateToken(
            user, jwtKey, issuer, audience, expiryMinutes);

        return Result<AuthResponseDto>.Success(
            user.ToResponseDto(token));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _authRepository.GetByEmailAsync(
            email, cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !BCrypt.Net.BCrypt.Verify(
                dto.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure(
                "Invalid email or password",
                ResultErrorType.Unauthorized);
        }

        if (!TryGetJwtSettings(
            out var jwtKey,
            out var issuer,
            out var audience,
            out var expiryMinutes))
        {
            return Result<AuthResponseDto>.Failure(
                "JWT configuration is invalid",
                ResultErrorType.Failure);
        }

        var token = GenerateToken(
            user, jwtKey, issuer, audience, expiryMinutes);

        return Result<AuthResponseDto>.Success(
            user.ToResponseDto(token));
    }

    
    private bool TryGetJwtSettings(
    out string jwtKey,
    out string issuer,
    out string audience,
    out double expiryMinutes)
    {
        jwtKey = _configuration["Jwt:Key"] ?? string.Empty;
        issuer = _configuration["Jwt:Issuer"] ?? string.Empty;
        audience = _configuration["Jwt:Audience"] ?? string.Empty;
        expiryMinutes = 0;

        var expiryValue = _configuration["Jwt:ExpiryMinutes"];

        return
            !string.IsNullOrWhiteSpace(jwtKey) &&
            Encoding.UTF8.GetByteCount(jwtKey) >= 32 &&
            !string.IsNullOrWhiteSpace(issuer) &&
            !string.IsNullOrWhiteSpace(audience) &&
            double.TryParse(
                expiryValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out expiryMinutes) &&
            expiryMinutes > 0;
    }

    private static string GenerateToken(
        UserEntity user,
        string jwtKey,
        string issuer,
        string audience,
        double expiryMinutes)
    {
        // var claims = new[]
        // {
        //     new Claim(
        //         ClaimTypes.NameIdentifier,
        //         user.Id.ToString()),

        //     new Claim(ClaimTypes.Name, user.Name),

        //     new Claim(ClaimTypes.Email, user.Email)
        // };

        var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(ClaimTypes.Name, user.Name),

                new Claim(ClaimTypes.Email, user.Email),

                new Claim(
                    ClaimTypes.Role,
                    ((UserRole)user.Role).ToString())
            };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}