using SQLite;
using RSVP_App.Models;

namespace RSVP_App.Services;

public partial class DatabaseService
{
    private SQLiteAsyncConnection? _db;

    public async Task InitAsync()
    {
        if (_db != null)
            return;
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "rsvp_app.db3");
        _db = new SQLiteAsyncConnection(databasePath);

        await _db.CreateTableAsync<UserAccount>();
        await _db.CreateTableAsync<EventItem>();
        await _db.CreateTableAsync<RsvpItem>();

        await SeedIfEmptyAsync();
    }

    private async Task SeedIfEmptyAsync()
    {
        if (_db == null) return;

        var userCount = await _db.Table<UserAccount>().CountAsync();
        if (userCount > 0) return;

        //Seed with a default user account for testing purposes.
        var host = new UserAccount
        {
            UserId = 0,
            Name = "Will Hughes",
            Email = "wilhug8130@students.ecpi.edu",
            Password = "Password1",
            Phone = "571-295-1980"
        };

        await _db.InsertAsync(host);

        //Seed with a default event for testing purposes.
        var now = DateTime.UtcNow;
        await _db.InsertAsync(new EventItem
        {
            Title = "Birthday Party",
            Description = "Join us for a fun birthday celebration! Food, drinks, and games provided. RSVP by June 20th.",
            Location = "123 Main St, Anytown, USA",
            HostUserId = host.UserId,
            StartUtc = now.AddDays(7).ToString("o"),
            EndUtc = now.AddDays(7).AddHours(4).ToString("o")
        });

        await _db.InsertAsync(new EventItem
        {
            Title = "Networking Mixer",
            Description = "Connect with professionals in your industry at our networking mixer. Light refreshments provided. RSVP by June 25th.",
            Location = "456 Elm St, Anytown, USA",
            HostUserId = host.UserId,
            StartUtc = now.AddDays(14).ToString("o"),
            EndUtc = now.AddDays(14).AddHours(2).ToString("o")
        });
    }

    public async Task<List<EventItem>> GetAllEventsAsync()
    {
        await InitAsync();
        return await _db.Table<EventItem>().OrderBy(e => e.StartUtc).ToListAsync();
    }   

    public async Task<List<EventItem>> GetAcceptedEventsForUserAsync(int userId)
    {
        await InitAsync();
        //Pull RSVP Rows for this user + accepted status
        var rsvps = await _db.Table<RsvpItem>()
            .Where(r => r.UserId == userId && r.Status == 1)
            .ToListAsync();

        //Extract EventIds from those RSVPs
        var acceptedEventIds = rsvps.Select(rsvps => rsvps.EventId).Distinct().ToList();

        if (acceptedEventIds.Count == 0)
            return new List<EventItem>();

        //Pull EventItems and filter in memory
        var allEvents = await _db.Table<EventItem>().ToListAsync();

        return allEvents.Where(e => acceptedEventIds.Contains(e.EventId)).OrderBy(e => e.StartUtc).ToList();
    }

    public async Task<List<EventItem>> GetPendingEventsForUserAsync(int userId)
    {
        await InitAsync();
        //Pull all RSVP rows for this user + any status except accepted (pending or declined)
        var rsvps = await _db!.Table<RsvpItem>()
            .Where(r => r.UserId == userId && r.Status != 1)
            .ToListAsync();
        
        var respondedIds = rsvps.Select(r => r.EventId).Distinct().ToList();

        //All events variable with accepted filtered out
        var allEvents = await _db.Table<EventItem>().OrderBy(e => e.StartUtc).ToListAsync();

        return allEvents.Where(e => !respondedIds.Contains(e.EventId)).ToList();
    }

    public async Task<UserAccount?> GetUserByEmailAsync(string email)
    {
        await InitAsync();
        return await _db!.Table<UserAccount>().Where(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<EventItem?> GetEventByIdAsync(int eventId)
    {
        await InitAsync();
        return await _db!.Table<EventItem>().Where(e => e.EventId == eventId).FirstOrDefaultAsync();
    }

    public async Task<UserAccount?> GetUserByIdAsync(int userId)
    {
        await InitAsync();
        return await _db!.Table<UserAccount>().Where(u => u.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<int> AddEventAsync(EventItem ev)
    {
        await InitAsync();
        return await _db!.InsertAsync(ev);
    }

    public async Task UpdateRsvpAsync(RsvpItem rsvp)
    {
        await InitAsync();

        //If logged in user already RSVP'd for this event, update existing record
        if (rsvp.UserId.HasValue)
        {
            var existing = await _db!.Table<RsvpItem>()
                .Where(r => r.UserId == rsvp.UserId && r.EventId == rsvp.EventId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Status = rsvp.Status;
                existing.PartySize = rsvp.PartySize;
                existing.Notes = rsvp.Notes;
                existing.CreatedUtc = DateTime.UtcNow.ToString("o");

                await _db.UpdateAsync(existing);
                return;
            }
        }

        //Guest RSVP or first RSVP for a logged in user
        rsvp.CreatedUtc = DateTime.UtcNow.ToString("o");
        await _db!.InsertAsync(rsvp);
    }

}
