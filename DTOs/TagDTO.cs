using System.Text.Json.Serialization;
using LoggerApp.Models;

namespace LoggerApp.DTOs
{
    public class TagDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        [JsonPropertyName("type_name")]
        public string TypeName { get; set; }

        [JsonPropertyName("logs")]
        public List<LogDTO> Logs { get; set; }
    }

    public class TagCreateDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }
    }
    public class TagUpdateDTO
    {
      
        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }
    }
}