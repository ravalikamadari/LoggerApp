using LoggerApp.DTOs;

namespace LoggerApp.Models;
public record Log
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string StackTrace { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long UpdatedByUserId { get; set; }
    public bool PartiallyDeleted { get; set; }
    public string Seen { get; set; }
    public string TagName { get; set; }
    public string TagType { get; set; }

    public LogDTO asDto => new LogDTO
    {
        Id = Id,
        Title = Title,
        Description = Description,
        StackTrace = StackTrace,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt,
        UpdatedByUserId = UpdatedByUserId,
        PartiallyDeleted = PartiallyDeleted,
        Seen = Seen,
        TagName = TagName,
        TagType = TagType

    };
}