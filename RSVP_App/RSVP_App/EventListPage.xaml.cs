using RSVP_App.Services;
using RSVP_App.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RSVP_App
{
	public partial class EventListPage : ContentPage
	{
		public ObservableCollection<EventItem> PendingEvents { get; set; } = new();
		public ObservableCollection<EventItem> AcceptedEvents { get; set; } = new();
		public ICommand RsvpCommand { get; }
		public ICommand DetailsCommand { get; }

		public string PendingCountText => $"{PendingEvents.Count} pending";
		public string AcceptedCountText => $"{AcceptedEvents.Count} accepted";

		public bool IsPendingEmpty => PendingEvents.Count == 0;
		public bool IsAcceptedEmpty => AcceptedEvents.Count == 0;

		public bool IsPendingNotEmpty => PendingEvents.Count > 0;
		public bool IsAcceptedNotEmpty => AcceptedEvents.Count > 0;

		public bool CanAddEvent => AppSession.isLoggedIn && !AppSession.isGuest;

		public EventListPage()
		{
			InitializeComponent();

			RsvpCommand = new Command<EventItem>(async (ev) => await GotoRsvp(ev));
			DetailsCommand = new Command<EventItem>(async (ev) => await GotoDetails(ev));

			BindingContext = this;
			OnPropertyChanged(nameof(CanAddEvent));

			LoadHardCodedEvents();
		}

		private void LoadHardCodedEvents()
		{
			PendingEvents.Clear();
			AcceptedEvents.Clear();

			// HardCoded Pending Events
			PendingEvents.Add(new EventItem
			{
				EventId = 101,
				Title = "Study Group Meetup",
				Location = "Online(Teams)",
				StartUtc = "Tuesday 6:00pm"
			});

			PendingEvents.Add(new EventItem
			{
				EventId = 102,
				Title = "Project Brainstorming Session",
				Location = "Online(Zoom)",
				StartUtc = "Monday 4:00pm"
			});

			//Hardcoded Accepted Events
			AcceptedEvents.Add(new EventItem
			{
				EventId = 201,
				Title = "Weekly Team Sync",
				Location = "Online(Teams)"
			});

			AcceptedEvents.Add(new EventItem
			{
				EventId = 202,
				Title = "Client Presentation",
				Location = "Online(Zoom)",
				StartUtc = "Wednesday 2:00pm"
			});

			//Refresh the UI
			OnPropertyChanged(nameof(PendingCountText));
			OnPropertyChanged(nameof(AcceptedCountText));
			OnPropertyChanged(nameof(IsPendingEmpty));
			OnPropertyChanged(nameof(IsAcceptedEmpty));
			OnPropertyChanged(nameof(IsPendingNotEmpty));
			OnPropertyChanged(nameof(IsAcceptedNotEmpty));
		}

		private async Task GotoRsvp(EventItem ev)
		{
			if (ev == null) return;
			var navigationParameter = new Dictionary<string, object>
		{
			{ "EventId", ev.EventId },
			{ "Title", ev.Title },
			{ "Location", ev.Location }
		};
			await Shell.Current.GoToAsync("rsvp", navigationParameter);
		}

		private async Task GotoDetails(EventItem ev)
		{
			if (ev == null) return;
			await Shell.Current.GoToAsync($"eventdetails?EventId={ev.EventId}");
		}

		private async void OnProfileClicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync("profile");
		}

		private async void OnPendingSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is EventItem selectedEvent)
			{
				await GotoDetails(selectedEvent);
				((CollectionView)sender).SelectedItem = null; // Deselect after navigation
			}
		}

		private async void OnAcceptedSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is EventItem selectedEvent)
			{
				await GotoDetails(selectedEvent);
				((CollectionView)sender).SelectedItem = null; // Deselect after navigation
			}
		}

		private async void OnAddEventClicked(object sender, EventArgs e)
		{
			if(!AppSession.isLoggedIn || AppSession.isGuest)
			{
				await DisplayAlert(
					"Login Required",
					"Please log in to a valid account to host an event.",
					"Ok");
				return;
			}

			await Shell.Current.GoToAsync("addevent");
		}

		private async Task LoadFromDbAsync()
		{
			await App.Db.InitAsync();

			PendingEvents.Clear();
			AcceptedEvents.Clear();

			//Guest: show all events as accepted/scheduled
			if (!AppSession.isLoggedIn || AppSession.isGuest)
			{
				var allEvents = await App.Db.GetAllEventsAsync();
				foreach (var ev in allEvents)
					AcceptedEvents.Add(ev);
			}
			else
			{
				//Logged in user
				int userId = AppSession.UserId;

				var pending = await App.Db.GetPendingEventsForUserAsync(userId);
				var accepted = await App.Db.GetAcceptedEventsForUserAsync(userId);

				foreach (var ev in pending)
					PendingEvents.Add(ev);

				foreach (var ev in accepted)
					AcceptedEvents.Add(ev);
			}

			//Notify UI
			OnPropertyChanged(nameof(PendingEvents));
			OnPropertyChanged(nameof(AcceptedEvents));
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			AddEventButton.IsVisible = AppSession.isLoggedIn && !AppSession.isGuest && AppSession.UserId > 0;

			await LoadFromDbAsync();
		}

	}
}