namespace SQLMigrationAssistant.Application.DTOs
{
    public class MigrationResult
    {
        public bool IsSuccess { get; init; }
        public MigrationResponse? Response { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;

        public static MigrationResult Success(MigrationResponse response) =>
            new() { IsSuccess = true, Response = response };

        public static MigrationResult Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
