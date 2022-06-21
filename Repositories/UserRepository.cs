using Dapper;
using LoggerApp.DTOs;
using LoggerApp.Models;
using LoggerApp.Repositories;
using LoggerApp.Utilities;

namespace LoggerApp.Repositories;
public interface IUserRepository
{
    Task<User> Create(User Data);
    Task<User> GetByEmail(string email);
    Task<List<User>> GetAllUsers();
    Task<User> GetById(int id);
    Task<bool> UpdateUser(UserUpdateDTO data);
}
public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration)
    {
    }
    public async Task<User> Create(User Data)
    {
        var query = $@"INSERT INTO ""{TableNames.user}"" (name, email, hashed_password, is_superuser)
        VALUES(@Name, @Email, @HashedPassword, @IsSuperUser) RETURNING *";
        using (var con = NewConnection)
            return await con.QuerySingleOrDefaultAsync<User>(query, Data);
    }
    public async Task<List<User>> GetAllUsers()
    {
        var query = $@"SELECT * FROM ""{TableNames.user}""";
        using (var con = NewConnection)
            return (await con.QueryAsync<User>(query)).AsList();
    }
    public async Task<User> GetByEmail(string email)
    {
        var query = $@"SELECT * FROM ""{TableNames.user}"" WHERE email = @Email;";
        using (var con = NewConnection)
            return await con.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
    }
    public async Task<User> GetById(int id)
    {
        var query = $@"SELECT * FROM ""{TableNames.user}"" WHERE id = @Id;";
        using (var con = NewConnection)
            return await con.QuerySingleOrDefaultAsync<User>(query, new { Id = id });
    }
    public async Task<bool> UpdateUser(UserUpdateDTO data)
    {
        var query = $@"UPDATE ""{TableNames.user}"" SET status = @Status WHERE id = @Id";

        using (var con = NewConnection)
            return (await con.ExecuteAsync(query, data)) > 0;
    }
}