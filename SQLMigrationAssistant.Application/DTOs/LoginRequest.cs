using MediatR;

namespace SQLMigrationAssistant.Application.DTOs
{
    public class LoginRequest : IRequest<LoginResponse>
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
