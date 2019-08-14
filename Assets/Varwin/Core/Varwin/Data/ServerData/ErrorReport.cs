namespace Varwin.Data
{
    public class ErrorReportDto : IJsonSerializable
    {
        public string UserEmail { get; set; }
        public string UserKey { get; set; }
        public string Stacktrace { get; set; }
        public string[] ProcessLog { get; set; }
        public string[] ErrorLog { get; set; }
    }
}