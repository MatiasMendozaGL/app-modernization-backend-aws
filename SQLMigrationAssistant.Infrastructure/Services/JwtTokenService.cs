using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Domain;
using SQLMigrationAssistant.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings, ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            try
            {
                var secretKey = _jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT SecretKey not configured");
                var issuer = _jwtSettings.Issuer;
                var audience = _jwtSettings.Audience;
                var expiryMinutes = _jwtSettings.AccessTokenExpiryMinutes;

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim("user_id", user.UserId.ToString()),
                    new Claim("username", user.Username)
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogDebug("Access token generated for user {UserId} with expiry {ExpiryMinutes} minutes", user.UserId, expiryMinutes);
                return await Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user {UserId}", user.UserId);
                throw new InvalidOperationException("Failed to generate access token", ex);
            }
        }

        public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            try
            {
                var secretKey = _jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT SecretKey not configured");
                var issuer = _jwtSettings.Issuer;
                var audience = _jwtSettings.Audience;
                var expiryDays = _jwtSettings.RefreshTokenExpiryDays;

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim("token_type", "refresh"),
                    new Claim("user_id", user.UserId.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(expiryDays),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogDebug("Refresh token generated for user {UserId} with expiry {ExpiryDays} days", user.UserId, expiryDays);
                return await Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token for user {UserId}", user.UserId);
                throw new InvalidOperationException("Failed to generate refresh token", ex);
            }
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
        {
            try
            {
                var secretKey = _jwtSettings.SecretKey ?? throw new InvalidOperationException("JWT SecretKey not configured");
                var issuer = _jwtSettings.Issuer;
                var audience = _jwtSettings.Audience;

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return await Task.FromResult(principal);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                throw new UnauthorizedAccessException("Invalid token");
            }
        }

        public ClaimsPrincipal? ExtractClaimsFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract claims from token");
                return null;
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check token expiration");
                return true; // Consider expired if we can't read it
            }
        }
    }
}