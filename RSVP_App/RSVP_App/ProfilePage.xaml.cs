using RSVP_App.Services;
using System.Threading.Tasks;

namespace RSVP_App;

public partial class ProfilePage : ContentPage
{

	public string Name => AppSession.Name;
	public string Email => AppSession.Email;
	public string Phone => AppSession.Phone;

	public ProfilePage()
	{
		InitializeComponent();
		BindingContext = this;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

		//Refresh bindings in case login state changed
		OnPropertyChanged(nameof(Name));
		OnPropertyChanged(nameof(Email));
		OnPropertyChanged(nameof(Phone));
    }

	private async void OnLogOutClicked(object sender, EventArgs e)
	{
		AppSession.Logout();
		await Shell.Current.GoToAsync("//login");
	}
}