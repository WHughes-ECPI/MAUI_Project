using RSVP_App.Services;	
namespace RSVP_App;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

	private async void OnCreateAccountClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("createaccount");
	}

	private async void OnLoginClicked(object sender, EventArgs e)
	{
		string username = usernameEntry.Text;
		string password = passwordEntry.Text;
		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
		{
			await DisplayAlert("Error", "Please enter both email and password.", "OK");
			return;
		}
		bool isAuthenticated = await AuthenticateUser(username, password);
		if (isAuthenticated)
		{
			await Shell.Current.GoToAsync("mainpage");
		}
		else
		{
			await DisplayAlert("Error", "Invalid email or password.", "OK");
		}
	}

	private async void OnGuestLoginClicked(object sender, EventArgs e)
	{
		AppSession.LoginAsGuest();
		await Shell.Current.GoToAsync("mainpage");
    }

}