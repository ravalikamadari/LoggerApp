using Dapper;
using LoggerApp.Utilities;

namespace LoggerApp.Repositories;
public interface IUserLoginRepository
{
    Task SetLogin(long userId, string deviceId, string notificationToken = null, string UserAgent = null);
}
public class UserLoginRepository : BaseRepository, IUserLoginRepository
{
    public UserLoginRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task SetLogin(long userId, string deviceId, string notificationToken = null, string UserAgent = null)
    {
        var query = $@"INSERT INTO ""{TableNames.user_login}"" (user_id, device_id, notification_token, user_agent)
         VALUES (@userId, @deviceId, @notificationToken, @UserAgent)";

        var lastLoginQuery = $@"UPDATE ""{TableNames.user}"" SET last_login = NOW() WHERE id = @UserId";
        using (var con = NewConnection)
        {
            await con.QuerySingleOrDefaultAsync(query, new { UserId = userId, DeviceId = deviceId, NotificationToken = notificationToken, UserAgent = UserAgent });
            await con.ExecuteAsync(lastLoginQuery, new { UserId = userId });
        }

    }

}