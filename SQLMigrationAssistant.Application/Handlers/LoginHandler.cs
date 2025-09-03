using MediatR;
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.Application.Handlers
{
    internal class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(IAuthService authService,
            IJwtTokenService jwtTokenService,
            ILogger<LoginHandler> logger
            )
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }
        public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Processing login request for {Email}", request.Email);

            // Validate user credentials
            var user = await _authService.ValidateCredentialsAsync(request.Email, request.Password);

            // Generate tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user);
            var expiresIn = DateTime.UtcNow.AddHours(1);

            _logger.LogInformation("Login successful for user {Email} (ID: {UserId})", user.Email, user.UserId);

            return new LoginResponse(user, accessToken, refreshToken, expiresIn);
        }
    }
}
