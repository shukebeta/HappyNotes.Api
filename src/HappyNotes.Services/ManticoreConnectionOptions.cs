namespace HappyNotes.Services
{
    public class ManticoreConnectionOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DbType { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string HttpEndpoint { get; set; } = string.Empty;
    }
}