namespace RSVP_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("createaccount", typeof(CreateAccountPage));
            Routing.RegisterRoute("events", typeof(EventListPage));
            Routing.RegisterRoute("eventdetails", typeof(EventDetailsPage));
            Routing.RegisterRoute("addevent", typeof(AddEventPage));
            Routing.RegisterRoute("rsvp", typeof(RSVPPage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));
        }
    }
}
