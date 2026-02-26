using RSVP_App.Models;

namespace RSVP_App;

public partial class CreateAccountPage : ContentPage
{
	public CreateAccountPage()
	{
		InitializeComponent();
	}

	private async void OnBackToLoginClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
    }	

	private async void OnCreateAccountClicked(object sender, EventArgs e)
	{
		string name = nameEntry.Text;
		string password = passwordEntry.Text;
		string email = emailEntry.Text;
		string phone = phoneEntry.Text;
		string phoneNumber = phoneEntry.Text;
		string confirmPassword = confirmEntry.Text;

		if (string.IsNullOrWhiteSpace(name) ||
			string.IsNullOrWhiteSpace(password) ||
			string.IsNullOrWhiteSpace(email) ||
			string.IsNullOrWhiteSpace(password))
		{
			await DisplayAlert("Error", "Please fill in all fields.", "OK");
			return;
        }

		await App.Db.InitAsync();

		var existing = await App.Db.GetUserByEmailAsync(email);
		if (existing != null)
		{
			await DisplayAlert("Error", "An account with this email already exists.", "OK");
			return;
        }

		var user = new UserAccount
		{
			Name = name,
			Email = email,
			Phone = phoneNumber,
			Password = password,
			CreatedUtc = DateTime.UtcNow.ToString("o")
		};

		await App.Db.AddUserAsync(user);

		await DisplayAlert("Success", "Account created successfully! Please log in.", "OK");
		await Shell.Current.GoToAsync("..");
    }
}