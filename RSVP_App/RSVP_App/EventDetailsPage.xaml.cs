using Microsoft.Extensions.Logging;
using RSVP_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RSVP_App;

[QueryProperty(nameof(EventId), "EventId")]
public partial class EventDetailsPage : ContentPage
{
	private int _eventId;

	public string TitleText { get; set; } = "Loading....";
	public string StartUtc { get; set; } = "";
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
				_ = LoadFromDbAsync(_eventId);
			}
        }
	}

    public EventDetailsPage()
	{
		InitializeComponent();
		BindingContext = this;
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

			//Format Start/End Time
			DateTime? start = DateTime.TryParse(ev.StartUtc, out var s) ? s.ToLocalTime() : (DateTime?)null;
			DateTime? end = DateTime.TryParse(ev.EndUtc, out var e) ? e.ToLocalTime() : (DateTime?)null;

			StartUtc = start.HasValue
				? (end.HasValue
					? $"{start.Value:ddd, MMM d * h:mm tt} - {end.Value:ddd, MMM d * h:mm tt}"
					: $"{start.Value:ddd, MMM d * h:mm tt}")
					: "Time unavailable";
			//Host display
			var host = await App.Db.GetUserByIdAsync(ev.HostUserId);
			HostName = host != null ? $"Hosted by: {host.Name}" : $"Host UserId: {ev.HostUserId}";

			RefreshAll();
		}
		catch(Exception ex)
		{
			TitleText = "Error loading events";
			Description = ex.Message;
			RefreshAll();
		}
	}

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
			StartUtc = (start.HasValue && end.HasValue) ? $"When: {start.Value:ddd, MMM d yyyy h:mm tt} - {end.Value:h:mm tt}" : "When: Time Unavailable";


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
		OnPropertyChanged(nameof(StartUtc));
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