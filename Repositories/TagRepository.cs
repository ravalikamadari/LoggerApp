using Dapper;
using LoggerApp.Models;
using LoggerApp.Utilities;

namespace LoggerApp.Repositories;
public interface ITagRepository
{
    Task<Tag> Create(Tag Data);
    Task<Tag> GetTagById(int id);
    Task<List<Tag>> GetAllTags();
    Task<bool> Delete(long id);
    Task<List<Tag>> GetAllForUser(int UserId);
    Task<List<Tag>> GetAllForLog(int LogId);
    Task<bool> UpdateTag(int id, int typeId);
    Task<List<TagType>> GetAllTagType();
}

public class TagRepository : BaseRepository, ITagRepository
{
    public TagRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<Tag> Create(Tag Data)
    {
        var query = $@"INSERT INTO ""{TableNames.tag}"" (name, type_id)
        VALUES(@Name, @TypeId) RETURNING *";
        using (var con = NewConnection)
            return await con.QuerySingleOrDefaultAsync<Tag>(query, Data);
    }

    public async Task<bool> Delete(long id)
    {
        var query = $@"DELETE FROM {TableNames.tag} WHERE id = @Id";
        using (var con = NewConnection)
            return (await con.ExecuteAsync(query, new { Id = id })) > 0;
    }

    public async Task<List<Tag>> GetAllForLog(int LogId)
    {
        var query = $@"SELECT * FROM ""{TableNames.log_tag}"" lt
        LEFT JOIN ""{TableNames.tag}"" t ON t.id = lt.tag_id WHERE lt.log_id = @LogId";
        // $@"SELECT t.*, tt.name AS type_name FROM ""{TableNames.tag}"" t LEFT JOIN ""{TableNames.tag_type}"" tt ON tt.id = t.type_id WHERE t.id = @Id;";
        using (var con = NewConnection)
            return (await con.QueryAsync<Tag>(query, new { LogId })).AsList();
    }

    public async Task<List<Tag>> GetAllForUser(int UserId)
    {
        var query = $@"SELECT * FROM ""{TableNames.user_tag}"" ut
        LEFT JOIN ""{TableNames.tag}"" t ON t.id = ut.tag_id WHERE ut.user_id = @UserId";
        using (var con = NewConnection)
            return (await con.QueryAsync<Tag>(query, new { UserId })).AsList();
    }

    public async Task<List<Tag>> GetAllTags()
    {
        var query = $@"SELECT * FROM ""{TableNames.tag}"" ORDER BY id";
        using (var con = NewConnection)
            return (await con.QueryAsync<Tag>(query)).AsList();
    }

    public async Task<List<TagType>> GetAllTagType()
    {
        var query = $@"SELECT * FROM ""{TableNames.tag_type}"" ORDER BY id";
        using (var con = NewConnection)
            return (await con.QueryAsync<TagType>(query)).AsList();
    }

    public async Task<Tag> GetTagById(int id)
    {
        var query = $@"SELECT t.*, tt.name AS type_name FROM ""{TableNames.tag}"" t LEFT JOIN ""{TableNames.tag_type}"" tt ON tt.id = t.type_id WHERE t.id = @Id;";
        using (var con = NewConnection)
            return await con.QuerySingleOrDefaultAsync<Tag>(query, new { Id = id });
    }

    public async Task<bool> UpdateTag(int id, int typeId)
    {
        var query = $@"UPDATE ""{TableNames.tag}"" SET type_id = @TypeId WHERE id = @Id";
        using (var con = NewConnection)
            return (await con.ExecuteAsync(query, new { Id = id, TypeId = typeId })) > 0;
    }
}