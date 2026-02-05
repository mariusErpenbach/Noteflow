namespace Noteflow.Models
{
    public class IndexCard
    {
        public int Id { get; set; }
        public string Front { get; set; } = string.Empty;
        public string Back { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
    }
}
