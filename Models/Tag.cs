using LoggerApp.DTOs;

namespace LoggerApp.Models;
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; }

    public TagDTO asDto => new TagDTO
    {
        Id = Id,
        Name = Name,
        CreatedAt = CreatedAt,
        TypeId = TypeId,
        TypeName = TypeName
    };
}

public record TagType
{
    public int Id { get; init; }
    public string Name { get; set; }
}

