using SlipperIS.Core.Models;

namespace SlipperIS.UI;

public static class CurrentSession
{
    public static User? CurrentUser { get; set; }

    public static bool IsLoggedIn => CurrentUser != null;

    public static int CurrentUserId => CurrentUser?.Id ?? 1;

    public static string CurrentUserName => CurrentUser?.FullName ?? "Guest";

    public static void Clear()
    {
        CurrentUser = null;
    }
}
