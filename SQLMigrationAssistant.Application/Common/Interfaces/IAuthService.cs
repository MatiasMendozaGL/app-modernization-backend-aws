using SQLMigrationAssistant.Domain;

namespace SQLMigrationAssistant.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password);
    }
}