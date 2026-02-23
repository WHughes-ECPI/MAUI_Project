using RSVP_App.Models;
using RSVP_App.Services;
using System.Threading.Tasks;

namespace RSVP_App;

[QueryProperty(nameof(EventId), "EventID")]
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
    public string Name  {get; set; } = "";
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
        //Logged in
        if(AppSession.isLoggedIn && !AppSession.isGuest)
        {
            Name = AppSession.Name ?? string.Empty;
            email = AppSession.Email ?? string.Empty;
            Phone = AppSession.Phone ?? string.Empty;
        }
        else
        {
            Name = string.Empty;
            email = string.Empty;
            Phone = string.Empty;
        }

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(email));
        OnPropertyChanged(nameof(Phone));
    }

    //HardCoded event for testing and fail catch
    private async Task LoadEventAsync(int eventId)
    {
        try
        {
            await App.Db.InitAsync();

            var ev = await App.Db.GetEventByIdAsync(eventId);

            if (ev == null)
            {
                TitleText = "Event not found";
                StartUtc = "";
                Location = "";
                HostName = "";
                Description = "";
                RefreshAll();
                return;
            }

            TitleText = ev.Title ?? "";
            Location = ev.Location ?? "";
            Description = ev.Description ?? "";

            DateTime? start = DateTime.TryParse(ev.StartUtc, out var s) ? s.ToLocalTime() : (DateTime?)null;
            DateTime? end = DateTime.TryParse(ev.EndUtc, out var e) ? e.ToLocalTime() : (DateTime?)null;

            WhenText = start.HasValue
            ? (end.HasValue ? $"{start.Value:ddd, MMM d * h:mm tt} - {end.Value:h:mm tt}" : $"{start.Value:ddd, MMM d * h:mm tt}")
            : "Time unavailable";
            RefreshAll();
        }
        catch (Exception ex)
        {
            TitleText = "Error loading event";
            Description = ex.Message;
            RefreshAll();
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
        //Validate fields
        if(string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(Phone))
        {
            await DisplayAlert("Error", "Please fill in all required fields (Name, Email, Phone).", "OK");
            return;
        }
        if(_eventId <= 0)
        {
            await DisplayAlert("Error", "Invalid Event ID. Cannot save RSVP.", "OK");
            return;
        }

        if(!int.TryParse(PartySizeText, out var partySize) || partySize < 1 || partySize > 20)
        {
            await DisplayAlert("Error", "Please enter a valid party size between 1 and 20.", "OK");
            return;
        }

        //Map Picker selection to RSVP status value (0=Accept, 1=Decline, 2=Maybe)
       int rsvpStatus = SelectedStatusIndex switch
        {
            0 => 1, //Accept -> 1 in DB
            1 => 2, //Decline -> 2 in DB
            2 => 3, //Maybe -> 3 in DB
            _ => 0 //Default/Invalid selection -> 0 (not set) in DB
        };

        if(AppSession.isLoggedIn && !AppSession.isGuest)
        {
            //Save RSVP to DB for logged in user
            var rsvp = new RsvpItem
            {
                EventId = _eventId,
                UserId = AppSession.UserId,
                Status = rsvpStatus,
                PartySize = partySize,
                Notes = string.IsNullOrWhiteSpace(NotesText) ? null : NotesText.Trim()
            };
            try
            {
                await App.Db.UpdateRsvpAsync(rsvp);
                await DisplayAlert("Success", "Your RSVP has been saved.", "OK");

                //Return to Event List
                await Shell.Current.GoToAsync("//events");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while saving your RSVP: {ex.Message}", "OK");
                return;
            }
        }
        else
        {
            //For guests or not logged in users, we could either prompt them to log in or save the RSVP locally (not implemented here)
            await DisplayAlert("Notice", "You are not logged in. Your RSVP will not be saved. Please log in to save your RSVP.", "OK");
        }
        await Shell.Current.GoToAsync($"rsvp?EventID={_eventId}");
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