using SQLite;

namespace RSVP_App.Models;

public class RsvpItem
{
    [PrimaryKey, AutoIncrement]
    public int RsvpId { get; set; }

    [NotNull]
    public int EventId { get; set; }

    public int? UserId { get; set; } // Nullable to allow for guests
    public string? GuestName { get; set; } // For guests, store their name here
    public string? GuestEmail { get; set; } // For guests, store their email here
    public string? GuestPhone { get; set; } // For guests, store their phone here

    [NotNull]
    public int Status { get; set; } // 0 = Pending, 1 = Accepted, 2 = Declined

    [NotNull]
    public int PartySize { get; set; } = 1;


    public string? Notes { get; set; }

    [NotNull]
    public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");

}
