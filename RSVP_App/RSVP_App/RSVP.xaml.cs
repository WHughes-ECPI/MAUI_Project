using RSVP_App.Models;
using RSVP_App.Services;

namespace RSVP_App;

[QueryProperty(nameof(EventId), "EventID")]
public partial class RSVP : ContentPage
{
    private int _eventId;

    public string TitleText { get; set; } = "Loading....";
    public string WhenText { get; set; } = "";
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
                _ = LoadEventandPrefillAsync();
            }
            else
            {
                //No valid event id, just load the default values (for testing)
                LoadEvent(0);
            }
        }
    }

    public RSVP()
    {
        InitializeComponent();

        LoadEvent(0);
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
    private void LoadEvent(int eventId)
    {
        var events = GetHardCodedEvents();

        var ev = events.FirstOrDefault(e => e.EventId == eventId) ?? events.First();

        TitleText = ev.Title;
        WhenText = ev.WhenWhere;
        Location = ev.Location;
        HostName = ev.HostName;
        Description = ev.Description;

        //Refresh the UI
        OnPropertyChanged(nameof(TitleText));
        OnPropertyChanged(nameof(WhenText));
        OnPropertyChanged(nameof(Location));
        OnPropertyChanged(nameof(HostName));
        OnPropertyChanged(nameof(Description));

        Title = "Event Details";
    }

    private static List<EventDetailsModel> GetHardCodedEvents() => new()
    {
        new EventDetailsModel
        {
            EventId = 101,
            Title = "Study Group Meetup",
            WhenWhere = "Tuesday, 6:00 PM * Online(Teams)",
            Location = "Online(Teams)",
            HostName = "Alice Johnson",
            Description = "Join us for a study group meetup to prepare for the upcoming exams. We'll cover key topics and share study tips."
        },
        new EventDetailsModel
        {
            EventId = 201,
            Title = "Weekly Team Sync",
            WhenWhere = "Monday, 10:00 AM * Online(Teams)",
            Location = "Online(Teams)",
            HostName = "Bob Smith",
            Description = "Our weekly team sync to discuss project updates, blockers, and next steps. Please come prepared with your status updates."
        }
    };

    private async Task LoadEventandPrefillAsync()
    {
            await App.Db.InitAsync();

            var ev = await App.Db.GetEventByIdAsync(_eventId);
            if (ev == null)
            {
                TitleText = "Event not found";
                Description = "No event found with the specified ID.";
                RefreshAll();
                return;
            }
            else
            {
                TitleText = ev.Title;

                DateTime? start = DateTime.TryParse(ev.StartUtc, out var startDt) ? startDt.ToLocalTime() : null;
                WhenText = start != null ? $"When: {start:ddd, MMM d yyyy h:mm tt}" : "When: Time Unavailable";
            }

            OnPropertyChanged(nameof(TitleText));
            OnPropertyChanged(nameof(WhenText));
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