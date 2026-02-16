using RSVP_App.Services;

namespace RSVP_App;

public partial class App : Application
{
    public static DatabaseService Db { get; } = new DatabaseService();
    public App()
    {
        InitializeComponent();
        _ = Db.InitAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}