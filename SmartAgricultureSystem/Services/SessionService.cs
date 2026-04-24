using System.IO;
using System;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// 会话管理服务（静态类）
    /// 负责本地持久化"记住登录"Token
    /// 使用本地文件模拟（生产环境可替换为注册表或加密文件）
    /// </summary>
    public static class SessionService
    {
        // Token存储文件路径
        private static readonly string TOKEN_FILE =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".session");

        /// <summary>
        /// 保存记住登录Token到本地文件
        /// </summary>
        /// <param name="token">Token字符串</param>
        public static void SaveRememberToken(string token)
        {
            try
            {
                // 简单Base64编码（生产环境应使用加密）
                string encoded = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(token));
                File.WriteAllText(TOKEN_FILE, encoded);
            }
            catch
            {
                // 忽略文件写入异常
            }
        }

        /// <summary>
        /// 从本地文件读取记住登录Token
        /// </summary>
        /// <returns>Token字符串，文件不存在返回null</returns>
        public static string LoadRememberToken()
        {
            try
            {
                if (!File.Exists(TOKEN_FILE)) return null;
                string encoded = File.ReadAllText(TOKEN_FILE);
                return System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(encoded));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 清除本地Token文件
        /// </summary>
        public static void ClearRememberToken()
        {
            try
            {
                if (File.Exists(TOKEN_FILE))
                {
                    File.Delete(TOKEN_FILE);
                }
            }
            catch
            {
                // 忽略异常
            }
        }
    }
}