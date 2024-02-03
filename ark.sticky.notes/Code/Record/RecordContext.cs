using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ark.bible.analysis
{
    public class Record
    {
        public long? record_id { get; set; }
        public string title { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string? color { get; set; }
        public string ip { get; set; }
        public DateTime? at { get; set; } = DateTime.UtcNow;
    }
    public class CurrentUser
    {
        public string? orig_email { get; set; }
        public string? email { get; set; }
        public string? ip { get; set; }
        public string? base_url { get; set; }
    }
    public class Secret
    {
        public long? secret_id { get; set; }
        public string? otp { get; set; }
        public bool? status { get; set; }
        public string ip { get; set; }
        public DateTime? at { get; set; } = DateTime.UtcNow;
        public string? error { get; set; }
        public string? email { get; set; }
    }
}
