namespace RSVP_App
{
    public partial class AppShell : Shell
    {

        
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("events", typeof(EventListPage));
            Routing.RegisterRoute("createaccount", typeof(CreateAccountPage));
            Routing.RegisterRoute("eventdetails", typeof(EventDetailsPage));
            Routing.RegisterRoute("addevent", typeof(AddEventPage));
            Routing.RegisterRoute("rsvp", typeof(RSVP));
            Routing.RegisterRoute("profile", typeof(ProfilePage));

            
        }

    }
}
