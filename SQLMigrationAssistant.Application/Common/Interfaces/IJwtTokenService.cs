using SQLMigrationAssistant.Domain;
using System.Security.Claims;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync(ApplicationUser user);
        Task<ClaimsPrincipal> ValidateTokenAsync(string token);
        ClaimsPrincipal? ExtractClaimsFromToken(string token);
        bool IsTokenExpired(string token);
    }
}