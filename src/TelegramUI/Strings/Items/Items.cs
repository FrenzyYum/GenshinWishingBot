using System.Text.Json.Serialization;

namespace TelegramUI.Strings.Items
{
    public class Items
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("typeId")]
        public string TypeId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("stars")]
        public int Stars { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}