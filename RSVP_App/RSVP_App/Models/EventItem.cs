using SQLite;

namespace RSVP_App.Models;

public class EventItem
{
    [PrimaryKey, AutoIncrement]
    public int EventId { get; set; }

    [NotNull]
    public int HostUserId { get; set; }

    [NotNull]
    public string HostName { get; set; } = string.Empty;

    [NotNull]
    public string Title { get; set; } = string.Empty;

    [NotNull]
    public string Description { get; set; } = string.Empty;

    [NotNull]
    public string Location { get; set; } = string.Empty;

    [NotNull]
    public string StartUtc { get; set; } = "";

    [NotNull]
    public string EndUtc { get; set; } = "";

    [NotNull]
    public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
}
