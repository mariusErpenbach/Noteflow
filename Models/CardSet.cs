using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Noteflow.Models
{
    public class CardSet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> CardIds { get; set; } = new List<int>();

        [JsonIgnore]
        public bool IsNew { get; set; }
    }
}
