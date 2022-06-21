using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LoggerApp.Models;

namespace LoggerApp.DTOs
{
    public class UserDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        // [JsonPropertyName("hashed_password")]
        // public string HashedPassword { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("last_login")]
        public DateTime LastLogin { get; set; }
        [JsonPropertyName("status")]
        public Status Status { get; set; }
        [JsonPropertyName("is_superuser")]
        public bool IsSuperUser { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag> Tags { get; set; }



    }

    public class UserCreateDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("is_superuser")]
        public bool IsSuperUser { get; set; }
    }
    public class UserUpdateDTO
    {
        [JsonPropertyName("id")]
        public Status Id { get; set; }
        [JsonPropertyName("status")]
        public Status Status { get; set; }
    }
    public class UserLoginDTO
    {
        [JsonPropertyName("email")]
        [Required]
        public string Email { get; set; }
        [JsonPropertyName("password")]
        [Required]
        public string Password { get; set; }
        [JsonPropertyName("device_id")]
        // [Required]
        public string DeviceId { get; set; }
        [JsonPropertyName("notification_token")]
        public string NotificationToken { get; set; } = null;
    }
    public class UserLoginResponseDTO
    {

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
        [JsonPropertyName("is_superuser")]
        public bool IsSuperuser { get; set; }
        public List<TagDTO> Tags { get; set; }



    }
}