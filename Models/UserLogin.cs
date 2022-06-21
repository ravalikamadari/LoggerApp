namespace LoggerApp.Models
{
    public record UserLogin
    {
        public int Id { get; set; }
        public string NotificationToken { get; init; }
    }
}