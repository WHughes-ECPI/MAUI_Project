using RSVP_App.Models;
using RSVP_App.Services;
using System.Runtime.CompilerServices;

namespace RSVP_App;

public partial class AddEventPage : ContentPage
{
	//Bound fields
	public string TitleText { get; set; } = string.Empty;
	public string DescriptionText { get; set; } = string.Empty;
	public string LocationText { get; set; } = string.Empty;

	public DateTime StartDate { get; set; } = DateTime.Today;
	public TimeSpan StartTime { get; set; } = new TimeSpan(19, 0, 0);

	public DateTime EndDate { get; set; } = DateTime.Today;
	public TimeSpan EndTime { get; set; } = new TimeSpan(19, 0, 0);

	public string StatusMessage { get; set; } = string.Empty;
	public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

	public AddEventPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	private async void OnCancelClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//events");
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{
		StatusMessage = "";
		OnPropertyChanged(nameof(StatusMessage));
		OnPropertyChanged(nameof(HasStatusMessage));

		//must be logged in to host an event
		if (!AppSession.isLoggedIn || AppSession.isGuest || AppSession.UserId <= 0)
		{
			StatusMessage = "You must be logged into a valid account to add an event.";
			OnPropertyChanged(nameof(StatusMessage));
			OnPropertyChanged(nameof(HasStatusMessage));
			return;
		}

		//Validate required fields
		if (string.IsNullOrEmpty(TitleText) ||
			string.IsNullOrEmpty(DescriptionText) ||
			string.IsNullOrEmpty(LocationText))
		{
			StatusMessage = "Title, Description, and Location are required.";
			OnPropertyChanged(nameof(StatusMessage));
			OnPropertyChanged(nameof(HasStatusMessage));
			return;
		}

		//Build DateTimes
		var startLocal = StartDate.Date + StartTime;
		var endLocal = EndDate.Date + EndTime;

		if(endLocal <= startLocal)
		{
			StatusMessage = "End date/time must be after Start date/time";
			OnPropertyChanged(nameof(StatusMessage));
			OnPropertyChanged(nameof(HasStatusMessage));
			return;
		}

		//Convert to UTC ISO strings for storage
		var startUTC = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();
		var endUTC = DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();

		var ev = new EventItem
		{
			HostUserId = AppSession.UserId,
			HostName = AppSession.Name ?? "Unknown Host",
            Title = TitleText.Trim(),
			Description = DescriptionText.Trim(),
			Location = LocationText.Trim(),
			StartUtc = startUTC.ToString("o"),
			EndUtc = endUTC.ToString("o"),
			CreatedUtc = DateTime.UtcNow.ToString("o")
		};

		try
		{
			await App.Db.InitAsync();
			await App.Db.AddEventAsync(ev);

			await DisplayAlert("Success", "Event saved.", "Ok");

			//return to event list refreshed with new event
			await Shell.Current.GoToAsync("//events");
		}
		catch(Exception ex)
		{
			StatusMessage = $"Save failed: {ex:Message}";
			OnPropertyChanged(nameof(StatusMessage));
			OnPropertyChanged(nameof(HasStatusMessage));
		}
	}
}