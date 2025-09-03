using Google;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using SQLMigrationAssistant.Application.Common.Interfaces;
using System.Net;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class RetryPolicy : IRetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly int _initialBackoffSeconds;
        private readonly int _maxBackoffSeconds;
        private readonly double _backoffMultiplier;
        private readonly HashSet<HttpStatusCode> _retryStatusCodes;
        private readonly HashSet<string> _retryableGrpcStatusCodes;
        private readonly ILogger<RetryPolicy> _logger;

        public RetryPolicy(
            int maxAttempts,
            int initialBackoffSeconds,
            int maxBackoffSeconds,
            double backoffMultiplier,
            IEnumerable<HttpStatusCode> retryStatusCodes,
            IEnumerable<string> retryableStatusCodes,
            ILogger<RetryPolicy> logger)
        {
            _maxAttempts = maxAttempts;
            _initialBackoffSeconds = initialBackoffSeconds;
            _maxBackoffSeconds = maxBackoffSeconds;
            _backoffMultiplier = backoffMultiplier;
            _retryStatusCodes = new HashSet<HttpStatusCode>(retryStatusCodes);
            _retryableGrpcStatusCodes = new HashSet<string>(retryableStatusCodes);
            _logger = logger;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            Exception lastException = null;
            double currentBackoffSeconds = _initialBackoffSeconds;

            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (IsRetryableException(ex) && attempt < _maxAttempts)
                {
                    lastException = ex;

                    _logger.LogWarning("Attempt {Attempt} of {MaxAttempts} failed: {Message}. Retrying in {Delay} seconds...",
                        attempt, _maxAttempts, ex.Message, currentBackoffSeconds);

                    await Task.Delay(TimeSpan.FromSeconds(currentBackoffSeconds));

                    // Calculate next backoff delay with exponential backoff
                    currentBackoffSeconds = Math.Min(currentBackoffSeconds * _backoffMultiplier, _maxBackoffSeconds);
                }
            }

            // If we get here, all attempts failed
            _logger.LogError("All {MaxAttempts} retry attempts failed. Last exception: {Exception}",
                _maxAttempts, lastException?.Message);

            throw lastException ?? new InvalidOperationException("Retry operation failed with no recorded exception");
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            await ExecuteAsync(async () =>
            {
                await operation();
                return true; // Dummy return for generic method
            });
        }


        private bool IsRetryableException(Exception ex)
        {
            return ex switch
            {
                RpcException rpcEx => IsRetryableRpcException(rpcEx),
                GoogleApiException apiEx => IsRetryableApiException(apiEx),
                TaskCanceledException => true,
                TimeoutException => true,
                HttpRequestException => true,
                System.Net.Sockets.SocketException => true,
                System.Net.NetworkInformation.NetworkInformationException => true,
                _ => false
            };
        }

        private bool IsRetryableRpcException(RpcException ex)
        {
            // Check if the status code string is in our configured retryable status codes
            var statusCodeString = ex.StatusCode.ToString();
            if (_retryableGrpcStatusCodes.Contains(statusCodeString))
            {
                return true;
            }

            // Fallback to default retryable status codes
            return ex.StatusCode switch
            {
                StatusCode.DeadlineExceeded => true,
                StatusCode.Unavailable => true,
                StatusCode.ResourceExhausted => true,
                StatusCode.Aborted => true,
                StatusCode.Internal => true,
                StatusCode.PermissionDenied => true,
                _ => false
            };
        }

        private bool IsRetryableApiException(GoogleApiException ex)
        {
            return _retryStatusCodes.Contains(ex.HttpStatusCode);
        }
    }
}