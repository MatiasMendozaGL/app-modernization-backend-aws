using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Domain;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApplicationUser> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                var credentialsList = GetUsersFromConfiguration();

                foreach (var credentials in credentialsList)
                {
                    if (credentials.TryGetValue("user", out var userEmail) &&
                        credentials.TryGetValue("hash", out var hash) &&
                        userEmail.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                        VerifyPassword(password, hash))
                    {
                        var user = new ApplicationUser(
                            userId: credentials.GetValueOrDefault("user_id", Guid.NewGuid().ToString()),
                            username: credentials.GetValueOrDefault("userName", ""),
                            email: credentials.GetValueOrDefault("user", "")
                        );

                        _logger.LogDebug("User {Email} credentials validated successfully", email);
                        return user;
                    }
                }

                _logger.LogWarning("Invalid credentials provided for email: {Email}", email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during credential validation for {Email}", email);
                throw;
            }
        }

        private List<Dictionary<string, string>> GetUsersFromConfiguration()
        {
            var users = new List<Dictionary<string, string>>();
            var usersSection = _configuration.GetSection("Users");

            foreach (var userSection in usersSection.GetChildren())
            {
                var userDict = new Dictionary<string, string>();
                foreach (var kvp in userSection.AsEnumerable())
                {
                    if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
                    {
                        var key = kvp.Key.Split(':').Last();
                        userDict[key] = kvp.Value;
                    }
                }
                if (userDict.Any())
                {
                    users.Add(userDict);
                }
            }

            return users;
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Using BCrypt for password hashing/verification
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}