namespace SlipperIS.UI.Helpers;

/// <summary>
/// 保存当前登录用户的会话信息。
/// 登录功能实现后，在登录成功时调用 <see cref="SetUser"/> 更新此状态。
/// </summary>
public static class CurrentSession
{
    /// <summary>当前登录用户的 ID（默认为管理员 1）</summary>
    public static int UserId { get; private set; } = 1;

    /// <summary>当前登录用户的用户名</summary>
    public static string Username { get; private set; } = "admin";

    /// <summary>
    /// 登录成功后调用，更新会话中的用户信息
    /// </summary>
    public static void SetUser(int userId, string username)
    {
        UserId = userId;
        Username = username;
    }
}
