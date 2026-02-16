namespace RSVP_App.Services;

    public class AppSession
    {
        public static bool isLoggedIn { get; set; } = false;
        public static bool isGuest { get; set; } = false;

    public static int UserId { get; set; } = 0; // Default to 0, can be set to a specific value for hardcoded user if needed
    public static string Name { get; set; } = string.Empty;
    public static string Email { get; set; } = string.Empty;
    public static string Phone { get; set; } = string.Empty;

    public static void LoginAsHardCodedUser()
    {
        isLoggedIn = true;
        isGuest = false;
        UserId = 0; // Set to 0 for hardcoded logged in user, can be set to a specific value if needed
        Name = "Will Hughes";
        Email = "wilhug8130@students.ecpi.edu";
        Phone = "571-295-1980";
    }

    public static void LoginAsGuest()
    {
        isLoggedIn = false;
        isGuest = true;
        UserId = 0; // Guest users do not have a UserId
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
