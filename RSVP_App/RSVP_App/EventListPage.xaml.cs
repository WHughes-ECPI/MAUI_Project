using RSVP_App.Models;
using RSVP_App.Services;
using System.Collections.ObjectModel;

namespace RSVP_App;

public partial class EventListPage : ContentPage
{
    private readonly SemaphoreSlim _loadLock = new(1, 1); // To prevent concurrent loads
    public ObservableCollection<EventItem> Events { get; set; } = new ObservableCollection<EventItem>();

    public List<string> FilterOptions { get; set; } = new() { "All Events" };
    public int SelectedFilterIndex { get; set; } = 0;

    public EventListPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        AddEventButton.IsVisible = AppSession.isLoggedIn && !AppSession.isGuest && AppSession.UserId > 0;
        //Build filter options
        if (AppSession.isLoggedIn && !AppSession.isGuest && AppSession.UserId > 0)
        {
            FilterOptions = new List<string> { "All Events", "Attending", "Hosting" };
        }
        else
        {
            FilterOptions = new List<string> { "All Events" };
        }

        SelectedFilterIndex = 0; // Reset to "All Events" when page appears

        OnPropertyChanged(nameof(FilterOptions));
        OnPropertyChanged(nameof(SelectedFilterIndex));

        await LoadEventsAsync();
    }

    private async void OnFilterChanged(object sender, EventArgs e)
    {
        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        if(!await _loadLock.WaitAsync(0)) // Try to acquire lock without waiting
        {
            return; // Another load is in progress, skip this one
        }
        try
        {
            await App.Db.InitAsync();
            Events.Clear();

            //Guest or not logged in -> All Events only
            if (!AppSession.isLoggedIn || AppSession.isGuest || AppSession.UserId <= 0)
            {
                var allEvents = await App.Db.GetAllEventsAsync();
                foreach (var ev in allEvents)
                {
                    Events.Add(ev);
                }
                return;
            }
            int userId = AppSession.UserId;

            if (SelectedFilterIndex == 0) //All
            {
                var allEvents = await App.Db.GetAllEventsAsync();
                foreach (var ev in allEvents)
                {
                    Events.Add(ev);
                }
            }
            else if (SelectedFilterIndex == 1) //Attending
            {
                var attendingEvents = await App.Db.GetAttendingEventByUserAsync(userId);
                foreach (var ev in attendingEvents)
                {
                    Events.Add(ev);
                }
            }
            else if (SelectedFilterIndex == 2) //Hosting
            {
                var hostingEvents = await App.Db.GetHostingEventByUserAsync(userId);
                foreach (var ev in hostingEvents)
                {
                    Events.Add(ev);
                }
            }
        }
        finally 
        {
            _loadLock.Release();
        }
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is EventItem selected)
        {
            await Shell.Current.GoToAsync($"eventdetails?EventId={selected.EventId}");
            ((CollectionView)sender).SelectedItem = null; // Deselect item after navigation
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("profile");
    }

    private async void OnAddEventClicked(object sender, EventArgs e)
    {
        if (!AppSession.isLoggedIn || AppSession.isGuest || AppSession.UserId <= 0)
        {
            await DisplayAlert("Access Denied", "You must be logged in to add an event.", "OK");
            return;
        }

        await Shell.Current.GoToAsync("addevent");
    }
}