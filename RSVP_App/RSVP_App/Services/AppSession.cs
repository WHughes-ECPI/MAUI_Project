namespace RSVP_App.Services;

    public class AppSession
    {
        public static bool isLoggedIn { get; set; } = false;
        public static bool isGuest { get; set; } = false;

    public static string Name { get; set; } = string.Empty;
    public static string Email { get; set; } = string.Empty;
    public static string Phone { get; set; } = string.Empty;

    public static void LoginAsHardCodedUser()
    {
        isLoggedIn = true;
        isGuest = false;
        Name = "Will Hughes";
        Email = "wilhug8130@students.ecpi.edu";
        Phone = "571-295-1980";
    }

    public static void LoginAsGuest()
    {
        isLoggedIn = false;
        isGuest = true;

        Name = "Guest User";
        Email = string.Empty;
        Phone = string.Empty;
    }

    public static void Logout()
    {
        isLoggedIn = false;
        isGuest = false;
        Name = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
    }
}
