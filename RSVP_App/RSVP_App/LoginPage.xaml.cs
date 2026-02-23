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
		string username = usernameEntry.Text?.Trim() ?? ""; 
		string password = passwordEntry.Text ?? "";

		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
		{
			await DisplayAlert("Error", "Please enter both email and password.", "OK");
			return;
		}
		if (username.Equals("hughes", StringComparison.OrdinalIgnoreCase))
		{
			username = "wilhug8130@students.ecpi.edu";
		}

		//Validate against DB
		var user = await AuthenticateUser(username, password);

		if (user == null)
		{
			await DisplayAlert(
				"Login Failed",
				"That account was not found or the password is incorrect.",
				"OK");

			//Stop here
			return;
		}

		//Success sets session and navigates to landing page
        AppSession.isLoggedIn = true;
        AppSession.isGuest = false;
        AppSession.UserId = user.UserId;
        AppSession.Name = user.Name;
        AppSession.Email = user.Email;
        AppSession.Phone = user.Phone;

        await Shell.Current.GoToAsync("//events");
    }

	private async Task<Models.UserAccount?> AuthenticateUser(string email, string password)
	{
		await App.Db.InitAsync();

		var user = await App.Db.GetUserByEmailAsync(email.Trim().ToLower());
		if (user == null) return null;

		return user.Password == password ? user : null;
    }

    private async void OnGuestLoginClicked(object sender, EventArgs e)
	{
		AppSession.LoginAsGuest();
		await Shell.Current.GoToAsync("//events");
    }

}