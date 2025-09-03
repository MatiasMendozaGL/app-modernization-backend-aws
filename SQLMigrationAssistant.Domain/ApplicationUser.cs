namespace SQLMigrationAssistant.Domain
{
    public class ApplicationUser
    {
        public string UserId { get; }
        public string Username { get; }
        public string Email { get; }

        public ApplicationUser(string userId, string username, string email)
        {
            UserId = userId;
            Username = username;
            Email = email;
        }
    }
}
