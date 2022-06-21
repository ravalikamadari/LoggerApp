using LoggerApp.DTOs;

namespace LoggerApp.Models;
public enum Status
{
    Active = 1,
    deactive = 0,
}
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string HashedPassword { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLogin { get; set; }
    public Status Status { get; set; }
    public bool IsSuperUser { get; set; }
    public bool DidLogout { get; set; }
    public string DeviceId { get; set; }
    public UserDTO asDto => new UserDTO
    {
        Id = Id,
        Name = Name,
        Email = Email,
        LastLogin = LastLogin,
        CreatedAt = CreatedAt,
        Status = Status,
        IsSuperUser = IsSuperUser,
    };
}