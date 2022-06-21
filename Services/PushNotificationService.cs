using System.Diagnostics;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using LoggerApp.Models;

namespace LoggerApp.Services;
public interface IPushNotificationService
{
    Task SendAll(List<PushNotificationData> data);
}

public class PushNotificationService : IPushNotificationService
{
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendAll(List<PushNotificationData> data)
    {

        if (data is null)
            return;

        _logger.LogInformation("Sending Push Notification");
        Stopwatch clock = Stopwatch.StartNew();

        try
        {
            List<Message> messagesData = new List<Message>();

            foreach (var item in data)
            {
                messagesData.Add(new Message
                {
                    Token = item.NotificationToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = "LogApp",
                        Body = "New Log Entry"
                    }
                });
            }
            await FirebaseMessaging.DefaultInstance.SendAllAsync(messagesData);
        }
        catch (Exception e)
        {
            _logger.LogError($"An error has occured in sending Push Notification: {e.Message}");
        }
        finally
        {
            clock.Stop();
            _logger.LogInformation($"Time taken for Push Notification Send All: {clock.Elapsed}");
        }
    }

}
