using SQLite;

namespace RSVP_App.Models;

public class UserAccount
{
    [PrimaryKey, AutoIncrement]
    public int UserId { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    [Unique, NotNull]
    public string Email { get; set; } = string.Empty;

    [NotNull]
    public string Password { get; set; } = string.Empty;

    [NotNull]
    public string Phone { get; set; } = string.Empty;

    [NotNull]
    public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o"); // ISO 8601 format

}
