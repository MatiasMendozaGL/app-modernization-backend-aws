namespace SQLMigrationAssistant.Application.Common.Exceptions
{
    public class MigrationException : Exception
    {
        public string MigrationId { get; }

        public MigrationException(string migrationId, string message) : base(message)
        {
            MigrationId = migrationId;
        }

        public MigrationException(string migrationId, string message, Exception innerException)
            : base(message, innerException)
        {
            MigrationId = migrationId;
        }
    }
}
