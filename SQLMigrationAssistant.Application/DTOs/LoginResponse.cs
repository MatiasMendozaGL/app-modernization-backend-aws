using SQLMigrationAssistant.Domain;

namespace SQLMigrationAssistant.Application.DTOs
{
    public class LoginResponse
    {
        public ApplicationUser User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresIn { get; set; }

        public LoginResponse(ApplicationUser user, string accessToken, string refreshToken, DateTime expiresIn)
        {
            User = user;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }
    }
}
