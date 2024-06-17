public static class DBManager
{
    public static string username;
    public static int score;
    public static int userid;
    public static bool online = false;
    public static bool ai = false;
    public static int difficulty = 0;
    public static bool LoggedIn { get { return username != null; } } //if username nothing == not login

    public static void LogOut() //so we can log out when we need to
    {
        username = null;
    }

}
