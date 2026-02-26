using RSVP_App.Models;
using RSVP_App.Services;
using System.Threading.Tasks;

namespace RSVP_App;

[QueryProperty(nameof(EventId), "EventId")]
public partial class RSVP : ContentPage
{
    private int _eventId;
    public bool IsLoggedInUser => AppSession.isLoggedIn && !AppSession.isGuest;
    public bool IsGuestUser => !IsLoggedInUser;

    //logged in display labels
    public string AccountName => $"Name: {AppSession.Name}";
    public string AccountEmail => $"Email: {AppSession.Email}";
    public string AccountPhone => $"Phone: {AppSession.Phone}";

    //Guest Entry Fields
    public string GuestName { get; set; } = "";
    public string GuestEmail { get; set; } = "";
    public string GuestPhone { get; set; } = "";

    //Event Details Fields
    public string TitleText { get; set; } = "Loading....";
    public string WhenText { get; set; } = "";
    public string StartUtc { get; set; } = "";
    public string Location { get; set; } = "";
    public string HostName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Name { get; set; } = "";
    public string email { get; set; } = "";
    public string Phone { get; set; } = "";


    //RSVP Fields
    public List<string> StatusOptions { get; } = new() { "Accept", "Decline", "Maybe" };
    public int SelectedStatusIndex { get; set; } = 0;

    public string PartySizeText { get; set; } = "1";
    public string NotesText { get; set; } = "";

    public string EventId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _eventId = id;
                _ = LoadEventandPrefillAsync(_eventId);
            }

        }
    }

    public RSVP()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void PrefillUserInfo()
    {
        if (AppSession.isLoggedIn && !AppSession.isGuest && AppSession.UserId > 0)
        {
            OnPropertyChanged(nameof(IsLoggedInUser));
            OnPropertyChanged(nameof(IsGuestUser));
            OnPropertyChanged(nameof(AccountName));
            OnPropertyChanged(nameof(AccountEmail));
            OnPropertyChanged(nameof(AccountPhone));
        }
    }
    private async Task LoadEventandPrefillAsync(int _eventId)
    {
        await App.Db.InitAsync();

        var ev = await App.Db.GetEventByIdAsync(_eventId);
        if (ev == null)
        {
            TitleText = "Event not found";
            Description = "No event found with the specified ID.";
            Location = "";
            WhenText = "";
            RefreshAll();
            return;
        }

        TitleText = ev.Title ?? "";
        Description = ev.Description ?? "";
        Location = ev.Location ?? "";

        DateTime? start = DateTime.TryParse(ev.StartUtc, out var s) ? s.ToLocalTime() : (DateTime?)null;
        DateTime? end = DateTime.TryParse(ev.EndUtc, out var e) ? e.ToLocalTime() : (DateTime?)null;

        WhenText = start.HasValue
            ? (end.HasValue ? $"{start.Value:ddd, MMM d * h:mm tt} - {end.Value:h:mm tt}" : $"{start.Value:ddd, MMM d * h:mm tt}")
            : "Time unavailable";
        PrefillUserInfo();
        OnPropertyChanged(nameof(IsLoggedInUser));
        OnPropertyChanged(nameof(IsGuestUser));
        OnPropertyChanged(nameof(AccountName));
        OnPropertyChanged(nameof(AccountEmail));
        OnPropertyChanged(nameof(AccountPhone));
        RefreshAll();
    }

    private void RefreshAll()
    {
        OnPropertyChanged(nameof(TitleText));
        OnPropertyChanged(nameof(WhenText));
        OnPropertyChanged(nameof(Location));
        OnPropertyChanged(nameof(HostName));
        OnPropertyChanged(nameof(Description));
    }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//events");
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_eventId <= 0)
        {
            await DisplayAlert("Error", "Invalid event ID. Cannot save RSVP.", "OK");
            return;
        }

        //Party Size
        if (!int.TryParse(PartySizeText, out var partySize) || partySize < 1 || partySize > 20)
        {
            await DisplayAlert("Error", "Please enter a valid party size between 1 and 20.", "OK");
            return;
        }

        //Map picker -> Status
        // 0=Maybe/Pending, 1=Accept, 2=Decline
        int status = SelectedStatusIndex switch
        {
            0 => 1, // Accept
            1 => 2, // Decline
            2 => 0, // Maybe
            _ => 0  // Default to Maybe/Pending if something goes wrong
        };

        await App.Db.InitAsync();

        //Logged-in: pre-populate and save under userId
        if(AppSession.isLoggedIn && !AppSession.isGuest && AppSession.UserId > 0)
        {
            var rsvp = new RsvpItem
            {
                EventId = _eventId,
                UserId = AppSession.UserId,
                Status = status,
                PartySize = partySize,
                Notes = NotesText,
                CreatedUtc = DateTime.UtcNow.ToString("o")
            };

            await App.Db.UpdateRsvpAsync(rsvp);
            await DisplayAlert("Success", "Your RSVP has been saved!", "OK");
            await Shell.Current.GoToAsync("//events");
            return;
        }

        //Guest: require guests to fill out fields and save with null userId and guest info
        if(string.IsNullOrWhiteSpace(GuestName) || 
            string.IsNullOrWhiteSpace(GuestEmail) || 
            string.IsNullOrWhiteSpace(GuestPhone))
        {
            await DisplayAlert("Error", "Please fill out all guest information fields.", "OK");
            return;
        }

        var guestRsvp = new RsvpItem
        {
            EventId = _eventId,
            UserId = null, // No user ID for guests
            GuestName = GuestName.Trim(),
            GuestEmail = GuestEmail.Trim(),
            GuestPhone = GuestPhone.Trim(),
            Status = status,
            PartySize = partySize,
            Notes = string.IsNullOrWhiteSpace(NotesText) ? null : NotesText.Trim(),
            CreatedUtc = DateTime.UtcNow.ToString("o")
        };

        await App.Db.UpdateRsvpAsync(guestRsvp);
        await DisplayAlert("Success", "Your RSVP has been saved!", "OK");
        await Shell.Current.GoToAsync("//events");
    }

    private class EventDetailsModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string WhenWhere { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

}