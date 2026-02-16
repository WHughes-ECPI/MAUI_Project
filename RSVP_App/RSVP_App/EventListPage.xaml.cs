using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RSVP_App
{
	public partial class EventListPage : ContentPage
	{
		public ObservableCollection<EventCard> PendingEvents { get; set; } = new();
		public ObservableCollection<EventCard> AcceptedEvents { get; set; } = new();
		public ICommand RsvpCommand { get; }
		public ICommand DetailsCommand { get; }

		public string PendingCountText => $"{PendingEvents.Count} pending";
		public string AcceptedCountText => $"{AcceptedEvents.Count} accepted";

		public bool IsPendingEmpty => PendingEvents.Count == 0;
		public bool IsAcceptedEmpty => AcceptedEvents.Count == 0;

		public bool IsPendingNotEmpty => PendingEvents.Count > 0;
		public bool IsAcceptedNotEmpty => AcceptedEvents.Count > 0;

		public EventListPage()
		{
			InitializeComponent();

			RsvpCommand = new Command<EventCard>(async (ev) => await GotoRsvp(ev));
			DetailsCommand = new Command<EventCard>(async (ev) => await GotoDetails(ev));

			BindingContext = this;

			LoadHardCodedEvents();
		}

		private void LoadHardCodedEvents()
		{
			PendingEvents.Clear();
			AcceptedEvents.Clear();

			// HardCoded Pending Events
			PendingEvents.Add(new EventCard
			{
				EventId = 101,
				Title = "Study Group Meetup",
				WhenWhere = "Tuesday, 6:00 PM * Online(Teams)"
			});

			PendingEvents.Add(new EventCard
			{
				EventId = 102,
				Title = "Project Brainstorming Session",
				WhenWhere = "Thursday, 3:00 PM * Online(Zoom)"
			});

			//Hardcoded Accepted Events
			AcceptedEvents.Add(new EventCard
			{
				EventId = 201,
				Title = "Weekly Team Sync",
				WhenWhere = "Monday, 10:00 AM * Online(Teams)"
			});

			AcceptedEvents.Add(new EventCard
			{
				EventId = 202,
				Title = "Client Presentation",
				WhenWhere = "Wednesday, 2:00 PM * Online(Zoom)"
			});

			//Refresh the UI
			OnPropertyChanged(nameof(PendingCountText));
			OnPropertyChanged(nameof(AcceptedCountText));
			OnPropertyChanged(nameof(IsPendingEmpty));
			OnPropertyChanged(nameof(IsAcceptedEmpty));
			OnPropertyChanged(nameof(IsPendingNotEmpty));
			OnPropertyChanged(nameof(IsAcceptedNotEmpty));
		}

		private async Task GotoRsvp(EventCard ev)
		{
			if (ev == null) return;
			var navigationParameter = new Dictionary<string, object>
		{
			{ "EventId", ev.EventId },
			{ "Title", ev.Title },
			{ "WhenWhere", ev.WhenWhere }
		};
			await Shell.Current.GoToAsync("rsvp", navigationParameter);
		}

		private async Task GotoDetails(EventCard ev)
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
			if (e.CurrentSelection.FirstOrDefault() is EventCard selectedEvent)
			{
				await GotoDetails(selectedEvent);
				((CollectionView)sender).SelectedItem = null; // Deselect after navigation
			}
		}

		private async void OnAcceptedSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is EventCard selectedEvent)
			{
				await GotoDetails(selectedEvent);
				((CollectionView)sender).SelectedItem = null; // Deselect after navigation
			}
		}


		public class EventCard
		{
			public int EventId { get; set; }
			public string Title { get; set; } = "";
			public string WhenWhere { get; set; } = "";
		}
	}
}