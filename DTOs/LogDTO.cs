using System.Text.Json.Serialization;

namespace LoggerApp.DTOs;
public class LogDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("stack_trace")]
    public string StackTrace { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("updated_by_user_id")]
    public long UpdatedByUserId { get; set; }
    [JsonPropertyName("partially_deleted")]
    public bool PartiallyDeleted { get; set; }

    [JsonPropertyName("seen")]
    public string Seen { get; set; }

    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    [JsonPropertyName("tag_type")]
    public string TagType { get; set; }


}

public class LogCreateDTO
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("stack_trace")]
    public string StackTrace { get; set; }

}
public class LogUpdateDTO
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("tag")]
    public long Tag { get; set; }

    [JsonPropertyName("type")]
    public long Type { get; set; }

}
public class UpdateLogSeenDTO
{
    [JsonPropertyName("is_seen")]
    public bool IsSeen { get; set; }

    [JsonPropertyName("log_ids")]
    public List<int> LogIds { get; set; }
}


