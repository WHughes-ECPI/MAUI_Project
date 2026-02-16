using Microsoft.Extensions.Logging;
using RSVP_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace RSVP_App;

[QueryProperty(nameof(EventId), "EventID")]
public partial class EventDetailsPage : ContentPage
{
	private int _eventId;

	public string TitleText { get; set; } = "Loading....";
	public string WhenText { get; set; } = "";
	public string Location { get; set; } = "";
	public string HostName { get; set; } = "";
    public string Description { get; set; } = "";

	public string EventId
	{
		set
		{
			if (int.TryParse(value, out var id))
			{
				_eventId = id;
				_=LoadFromDbAsync(_eventId);
            }
			else
			{
                //No valid event id, just load the default values (for testing)
                LoadEvent(0);
			}
        }
	}

    public EventDetailsPage()
	{
		InitializeComponent();

		LoadEvent(0);
		BindingContext = this;
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

	private async Task LoadFromDbAsync(int eventId)
	{
		try
		{
			await App.Db.InitAsync();

			EventItem? ev = await App.Db.GetEventByIdAsync(eventId);
			if(ev == null)
			{
				TitleText="Event not found";
				Description="No event found with the specified ID.";
				RefreshAll();
				return;
            }

			TitleText = ev.Title;
			Location = $"Location: {ev.Location}";
			Description = ev.Description;

			//Formatting Time
			DateTime? start = DateTime.TryParse(ev.StartUtc, out var startDt) ? startDt.ToLocalTime() : null;
			DateTime? end = DateTime.TryParse(ev.EndUtc, out var endDt) ? endDt.ToLocalTime() : null;
			WhenText = (start.HasValue && end.HasValue) ? $"When: {start.Value:ddd, MMM d yyyy h:mm tt} - {end.Value:h:mm tt}" : "When: Time Unavailable";


            //HostName is not currently used in the UI, here if I want to add it later.
            var host = await App.Db.GetUserByIdAsync(ev.HostUserId);
			HostName = host != null ? $"Hosted by: {host.Name}" : $"Host UserId: {ev.HostUserId}";
			RefreshAll();
        }
		catch (Exception ex)
		{
			TitleText = "Error loading event";
			Description = $"An error occurred while loading the event details: {ex.Message}";
			RefreshAll();
        }
    }

    private void RefreshAll()
	{
		OnPropertyChanged(nameof(TitleText));
		OnPropertyChanged(nameof(WhenText));
		OnPropertyChanged(nameof(Location));
		OnPropertyChanged(nameof(HostName));
		OnPropertyChanged(nameof(Description));
    }
	private async void OnBackClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//events");
    }

	private async void OnRsvpClicked(object sender, EventArgs e)
	{
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