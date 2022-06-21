using Dapper;
using Hangfire;
using LoggerApp.DTOs;
using LoggerApp.Models;
using LoggerApp.Services;
using LoggerApp.Utilities;

namespace LoggerApp.Repositories;

public interface ILogRepository
{
    void DeleteLogPermanently();
    Task<Log> Create(Log Data);
    Task<Log> GetLogById(long id);
    Task<bool> UpdateLog(Log data, long tag, long type);
    Task<List<Log>> GetAllLogs(PaginationDTO pagination, long userId, int? tagId = null, string title = null, DateTime? from = null, DateTime? to = null);
    Task<bool> Delete(long id);
    Task<List<Log>> GetAllForTag(int TagId);
    Task SetSeen(int id, int userId);
    Task<bool> GetSeen(int id, int userId);
    Task UnsetSeen(int id, int userId);
    Task SendPushNotification();
    Task<List<Log>> GetAllLogsForUser(long userId, PaginationDTO pagination, int? tagId, string title, DateTime? from, DateTime? to);
}
public class LogRepository : BaseRepository, ILogRepository
{
    private readonly IPushNotificationService _pushNotification;
    private readonly ILogger<LogRepository> _logger;
    public LogRepository(IConfiguration configuration, IPushNotificationService pushNotification, ILogger<LogRepository> logger) : base(configuration)
    {
        _pushNotification = pushNotification;
        _logger = logger;
    }
    public async Task<Log> Create(Log Data)
    {
        var query = $@"INSERT INTO ""{TableNames.log}"" (title, description, stack_trace)
        VALUES(@Title, @Description, @StackTrace) RETURNING *";
        using (var con = NewConnection)
            return await con.QuerySingleOrDefaultAsync<Log>(query, Data);

    }
    public async Task<bool> Delete(long Id)
    {
        var query = $@"UPDATE {TableNames.log} SET partially_deleted = true WHERE id = @Id";

        using (var con = NewConnection)
            return (await con.ExecuteAsync(query, new { Id })) > 0;
    }

    public void DeleteLogPermanently()
    {
        var query = $@"DELETE FROM {TableNames.log} WHERE created_at < NOW() - INTERVAL '90' Day";
        using (var con = NewConnection)
            Console.WriteLine("Removed outdated items:" + con.Execute(query));
    }

    public async Task<List<Log>> GetAllForTag(int TagId)
    {
        var query = $@"SELECT * FROM ""{TableNames.log_tag}"" lt
        LEFT JOIN ""{TableNames.log}"" l ON l.id = lt.log_id WHERE lt.tag_id = @TagId";
        using (var con = NewConnection)
            return (await con.QueryAsync<Log>(query, new { TagId })).AsList();
    }
    public async Task<List<Log>> GetAllLogs(PaginationDTO pagination, long userId, int? tagId = null, string title = null, DateTime? from = null, DateTime? to = null)
    {
        var query = $@"SELECT * FROM ""{TableNames.log}"" l WHERE";

        if (tagId is not null)
            query = query + $@" l.tag_id = @TagId AND";

        if (title is not null)
            query = query + $@" l.title LIKE '%{title}%' AND";

        if (from is not null && to is not null)
            query = query + @" l.created_at BETWEEN @FromDate AND @ToDate AND";

        query = query + @" NOT l.partially_deleted = true ORDER BY l.created_at LIMIT @Limit OFFSET @Offset";
        using (var con = NewConnection)
            return (await con.QueryAsync<Log>(query, new
            {
                Limit = pagination.Limit,
                Offset = pagination.Offset,
                Title = title,
                FromDate = from,
                ToDate = to,
                TagId = tagId,
                UserId = userId
            })).AsList();
    }

    public async Task<List<Log>> GetAllLogsForUser(long userId, PaginationDTO pagination, int? tagId, string title, DateTime? from, DateTime? to)
    {
        var query = $@"SELECT l.* FROM {TableNames.user_tag} ut
         LEFT JOIN {TableNames.log} l ON l.tag_id = ut.tag_id
         WHERE ut.user_id = @UserId AND";

        if (tagId is not null)
            query = query + $@" l.tag_id = @TagId AND";

        if (title is not null)
            query = query + $@" l.title LIKE '%{title}%' AND";

        if (from is not null && to is not null)
            query = query + @" l.created_at BETWEEN @FromDate AND @ToDate AND";

        query = query + @" NOT l.partially_deleted = true ORDER BY l.created_at LIMIT @Limit OFFSET @Offset";
        using (var con = NewConnection)
            return (await con.QueryAsync<Log>(query, new
            {
                Limit = pagination.Limit,
                Offset = pagination.Offset,
                Title = title,
                FromDate = from,
                ToDate = to,
                TagId = tagId,
                UserId = userId,
            })).AsList();
    }

    public async Task<Log> GetLogById(long id)
    {
        var query = $@"SELECT l.*, t.name AS tag_name, tt.name AS tag_type  FROM ""{TableNames.log}"" l
        LEFT JOIN {TableNames.tag} t ON t.id = l.tag_id
        LEFT JOIN {TableNames.tag_type} tt ON tt.id = l.tag_type_id WHERE l.id = @Id;";
        using (var con = NewConnection)
            return await con.QuerySingleOrDefaultAsync<Log>(query, new { Id = id });
    }
    public async Task<bool> GetSeen(int id, int userId)
    {
        var query = $@"SELECT * FROM ""{TableNames.log_seen}"" WHERE user_id = @UserId AND log_id = @LogId";
        using (var con = NewConnection)
        {
            var res = (await con.QuerySingleOrDefaultAsync(query, new { UserId = userId, LogId = id }));
            if (res is null)
                return true;
            else
                return false;
        }
    }

    public async Task SendPushNotification()
    {
        var query = $@"SELECT FROM {TableNames.user_login} WHERE notification_token != null";

        List<UserLogin> loggedUser = new List<UserLogin>();
        using (var con = NewConnection)
            loggedUser = (await con.QueryAsync<UserLogin>(query)).AsList();

        Console.WriteLine(loggedUser);

        var notificationData = new PushNotificationData
        {
            BodyText = "Resolve entry",
            TitleText = "You have got new log entry",
        };
        var pn = _pushNotification.SendAll(loggedUser.Select(x => notificationData with
        {
            NotificationToken = x.NotificationToken,
        }).AsList());
        _logger.LogInformation("Fired notification");
    }

    public async Task SetSeen(int id, int userId)
    {
        var query = $@"INSERT INTO ""{TableNames.log_seen}"" (log_id, user_id) VALUES(@Id, @UserId)";
        using (var con = NewConnection)
            (await con.QueryAsync<Log>(query, new { Id = id, UserId = userId })).AsList();

    }

    public async Task UnsetSeen(int id, int userId)
    {
        var query = $@"DELETE FROM {TableNames.log_seen} WHERE log_id = @LogId AND user_id = @UserId";
        using (var con = NewConnection)
            await con.ExecuteAsync(query, new { LogId = id, UserId = userId });

    }

    public async Task<bool> UpdateLog(Log data, long tag, long type)
    {
        var query = $@"UPDATE ""{TableNames.log}"" SET description = @Description, updated_at = NOW(), updated_by_user_id = @UpdatedByUserId, tag_id = @Tag, tag_type_id = @Type WHERE id = @Id";
        // var logTagDelete = $@"DELETE FROM ""{TableNames.log_tag}"" WHERE log_id = @Id;";
        // var logTagInsert = $@"INSERT INTO ""{TableNames.log_tag}"" (log_id, tag_id) VALUES(@LogId, @TagId)";

        using (var con = NewConnection)
            if ((await con.ExecuteAsync(query, new { Description = data.Description, UpdatedByUserId = data.UpdatedByUserId, Tag = tag, Type = type, Id = data.Id })) > 0)
            {
                return true;
                // if (tags is null) return true;
                // if ((await con.ExecuteAsync(logTagDelete, new { Id = data.Id })) > 0)
                // {
                //     foreach (var tagId in tags)
                //         await con.QuerySingleOrDefaultAsync(logTagInsert, new { LogId = data.Id, TagId = tagId });
                //     return true;
                // }
                // else
                //     return false;
            }
            else
                return false;
    }
}

