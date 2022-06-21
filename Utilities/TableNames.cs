namespace LoggerApp.Utilities;

public static class UserConstants
{
    public const string Name = nameof(Name);
    public const string Email = nameof(Email);
    public const string Id = nameof(Id);
    public const string IsSuperuser = nameof(IsSuperuser);
    public const string UserId = nameof(UserId);
}

public enum TableNames
{
    user,
    tag,
    log,
    log_tag,
    user_tag,
    tag_type,
    log_seen,
    user_login




}