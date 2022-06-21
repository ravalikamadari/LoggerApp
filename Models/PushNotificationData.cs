namespace LoggerApp.Models;

public record PushNotificationData
{
    public string NotificationToken { get; init; }
    public string BodyText { get; init; }
    public string TitleText { get; init; }
}