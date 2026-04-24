using System;
using System.Security.Cryptography;
using System.Text;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// 密码加密工具类
    /// 使用 SHA256 + 随机盐值 对密码进行哈希处理
    /// 防止彩虹表攻击
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// 生成随机盐值（32字节 Base64编码）
        /// </summary>
        /// <returns>Base64编码的盐值字符串</returns>
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            // 使用加密安全的随机数生成器
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// 对密码进行哈希处理
        /// 算法：SHA256(密码 + 盐值)
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <param name="salt">盐值</param>
        /// <returns>哈希后的密码字符串（十六进制）</returns>
        public static string HashPassword(string password, string salt)
        {
            // 将密码与盐值拼接
            string combined = password + salt;
            byte[] inputBytes = Encoding.UTF8.GetBytes(combined);

            // 执行SHA256哈希
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // 转换为十六进制字符串
                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 验证密码是否正确
        /// </summary>
        /// <param name="inputPassword">用户输入的密码</param>
        /// <param name="storedHash">数据库中存储的哈希值</param>
        /// <param name="salt">数据库中存储的盐值</param>
        /// <returns>密码是否匹配</returns>
        public static bool VerifyPassword(
            string inputPassword, string storedHash, string salt)
        {
            string inputHash = HashPassword(inputPassword, salt);

            // 【关键修改】调用我们手动实现的固定时间比较方法
            return FixedTimeEquals(
                Encoding.UTF8.GetBytes(inputHash),
                Encoding.UTF8.GetBytes(storedHash));
        }

        /// <summary>
        /// 验证密码强度
        /// 要求：长度≥8，包含大小写字母和数字
        /// </summary>
        /// <param name="password">待验证密码</param>
        /// <returns>验证结果消息，null表示验证通过</returns>
        public static string ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "密码不能为空";
            if (password.Length < 8)
                return "密码长度不能少于8位";
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
                return "密码必须包含至少一个大写字母";
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
                return "密码必须包含至少一个小写字母";
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
                return "密码必须包含至少一个数字";
            return null; // 验证通过
        }

        //手动实现固定时间比较（防止时序攻击）
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            // 1. 如果任一数组为 null 或长度不相等，直接返回 false
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            // 2. 逐字节进行异或比较
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            // 3. 只有当所有字节都相等时，diff 才会是 0
            return diff == 0;
        }
    }
}