using System;

namespace DeviceManage.Helpers
{
    /// <summary>
    /// 用于在应用程序范围内存储“当前登录用户”信息。
    /// 注意：该类只是简单示例，若未来需要多线程/多用户并发，可使用 AsyncLocal 或其它上下文方案。
    /// </summary>
    public static class CurrentUserContext
    {
        /// <summary>
        /// 当前登录用户 ID
        /// </summary>
        public static int? UserId { get; private set; }

        /// <summary>
        /// 当前登录用户名
        /// </summary>
        public static string? Username { get; private set; }

        /// <summary>
        /// 更新当前用户信息
        /// </summary>
        public static void Set(int? userId, string? username)
        {
            UserId = userId;
            Username = username;
        }

        /// <summary>
        /// 清空当前用户信息（如登出时）
        /// </summary>
        public static void Clear()
        {
            UserId = null;
            Username = null;
        }
    }
}
