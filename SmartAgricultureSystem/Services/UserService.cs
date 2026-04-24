using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// 用户业务服务
    /// 负责用户注册、查询、信息修改、账号管理等操作
    /// </summary>
    public class UserService
    {
        // 数据库连接
        private SQLiteAsyncConnection mConnection;

        // 数据库文件路径
        private static readonly string DB_PATH =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AgricultureData.db");

        /// <summary>
        /// 初始化数据库表，并创建默认管理员账号
        /// </summary>
        public async Task InitializeAsync()
        {
            mConnection = new SQLiteAsyncConnection(DB_PATH);
            await mConnection.CreateTableAsync<User>();
            await mConnection.CreateTableAsync<LoginRecord>();
            // 确保默认管理员存在
            await EnsureDefaultAdminAsync();
        }

        /// <summary>
        /// 创建默认管理员账号（admin / Admin@123456）
        /// 仅在数据库中没有任何管理员时执行
        /// </summary>
        private async Task EnsureDefaultAdminAsync()
        {
            var adminCount = await mConnection.Table<User>()
                .Where(u => u.role == UserRole.Admin)
                .CountAsync();

            if (adminCount == 0)
            {
                string salt = PasswordHelper.GenerateSalt();
                var admin = new User
                {
                    username = "admin",
                    passwordHash = PasswordHelper.HashPassword("Admin@123456", salt),
                    passwordSalt = salt,
                    nickname = "系统管理员",
                    role = UserRole.Admin,
                    createdAt = DateTime.Now
                };
                await mConnection.InsertAsync(admin);
            }
        }
        /// <summary>
        /// 注册新用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">原始密码</param>
        /// <param name="nickname">昵称</param>
        /// <param name="role">角色，默认农户</param>
        /// <returns>注册结果消息，null表示成功</returns>
        public async Task<string> RegisterAsync(
            string username, string password,
            string nickname, UserRole role = UserRole.Farmer)
        {

            // 验证用户名格式（4-20位字母数字下划线）
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                username, @"^[a-zA-Z0-9_]{4,20}$"))
            {
                return "用户名为4-20位字母、数字或下划线";
            }

            // 验证密码强度
            string pwdError = PasswordHelper.ValidatePasswordStrength(password);
            if (pwdError != null) return pwdError;

            // 检查用户名是否已存在
            var existing = await mConnection.Table<User>()
                .Where(u => u.username == username)
                .FirstOrDefaultAsync();
            if (existing != null) return "用户名已被注册";

            // 生成盐值并哈希密码
            string salt = PasswordHelper.GenerateSalt();
            var newUser = new User
            {
                username = username,
                passwordHash = PasswordHelper.HashPassword(password, salt),
                passwordSalt = salt,
                nickname = string.IsNullOrWhiteSpace(nickname) ? username : nickname,
                role = role,
                createdAt = DateTime.Now
            };

            await mConnection.InsertAsync(newUser);
            return null; // null 表示注册成功
        }

        /// <summary>
        /// 根据用户名查询用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户对象，不存在则返回null</returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await mConnection.Table<User>()
                .Where(u => u.username == username)
                .FirstOrDefaultAsync();
        }
        /// <summary>
        /// 根据ID查询用户
        /// </summary>
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await mConnection.GetAsync<User>(userId);
        }

        /// <summary>
        /// 获取所有用户列表（管理员专用）
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await mConnection.Table<User>().ToListAsync();
        }

        /// <summary>
        /// 更新用户昵称
        /// </summary>
        public async Task UpdateNicknameAsync(int userId, string newNickname)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.nickname = newNickname;
                await mConnection.UpdateAsync(user);
            }
        }

        /// <summary>
        /// 更新用户头像路径
        /// </summary>
        public async Task UpdateAvatarAsync(int userId, string avatarPath)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.avatarPath = avatarPath;
                await mConnection.UpdateAsync(user);
            }
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="oldPassword">旧密码</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>操作结果消息，null表示成功</returns>
        public async Task<string> ChangePasswordAsync(
            int userId, string oldPassword, string newPassword)
        {

            var user = await GetUserByIdAsync(userId);
            if (user == null) return "用户不存在";

            // 验证旧密码
            if (!PasswordHelper.VerifyPassword(
                oldPassword, user.passwordHash, user.passwordSalt))
            {
                return "原密码错误";
            }

            // 验证新密码强度
            string pwdError = PasswordHelper.ValidatePasswordStrength(newPassword);
            if (pwdError != null) return pwdError;

            // 新密码不能与旧密码相同
            if (PasswordHelper.VerifyPassword(
                newPassword, user.passwordHash, user.passwordSalt))
            {
                return "新密码不能与原密码相同";
            }

            // 更新密码
            string newSalt = PasswordHelper.GenerateSalt();
            user.passwordHash = PasswordHelper.HashPassword(newPassword, newSalt);
            user.passwordSalt = newSalt;
            await mConnection.UpdateAsync(user);
            return null;
        }

        /// <summary>
        /// 绑定手机号（模拟）
        /// </summary>
        public async Task<string> BindPhoneAsync(int userId, string phone)
        {
            // 验证手机号格式
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                phone, @"^1[3-9]\d{9}$"))
            {
                return "手机号格式不正确";
            }

            // 检查手机号是否被其他账号绑定
            var existing = await mConnection.Table<User>()
                .Where(u => u.phoneNumber == phone && u.id != userId)
                .FirstOrDefaultAsync();
            if (existing != null) return "该手机号已被其他账号绑定";

            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.phoneNumber = phone;
                await mConnection.UpdateAsync(user);
            }
            return null;
        }
        /// <summary>
        /// 锁定账号（管理员操作）
        /// </summary>
        /// <param name="userId">目标用户ID</param>
        /// <param name="lockMinutes">锁定分钟数，-1表示永久锁定</param>
        public async Task LockUserAsync(int userId, int lockMinutes = -1)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.isLocked = true;
                user.lockUntil = lockMinutes == -1
                    ? DateTime.MaxValue
                    : DateTime.Now.AddMinutes(lockMinutes);
                await mConnection.UpdateAsync(user);
            }
        }

        /// <summary>
        /// 解锁账号（管理员操作）
        /// </summary>
        public async Task UnlockUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.isLocked = false;
                user.lockUntil = null;
                user.failedLoginCount = 0;
                await mConnection.UpdateAsync(user);
            }
        }

        /// <summary>
        /// 记录登录日志
        /// </summary>
        public async Task RecordLoginAsync(
            string username, bool isSuccess, string failReason = null)
        {
            var record = new LoginRecord
            {
                username = username,
                loginTime = DateTime.Now,
                isSuccess = isSuccess,
                failReason = failReason
            };
            await mConnection.InsertAsync(record);
        }

        /// <summary>
        /// 更新用户登录失败次数，超过5次自动锁定30分钟
        /// </summary>
        public async Task HandleLoginFailAsync(User user)
        {
            user.failedLoginCount++;
            if (user.failedLoginCount >= 5)
            {
                user.isLocked = true;
                user.lockUntil = DateTime.Now.AddMinutes(30);
            }
            await mConnection.UpdateAsync(user);
        }
        /// <summary>
        /// 重置登录失败次数（登录成功后调用）
        /// </summary>
        public async Task ResetLoginFailAsync(User user)
        {
            user.failedLoginCount = 0;
            user.lastLoginAt = DateTime.Now;
            await mConnection.UpdateAsync(user);
        }

        /// <summary>
        /// 保存"记住登录"Token
        /// </summary>
        public async Task SaveRememberTokenAsync(
            int userId, string token, DateTime expireAt)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.rememberLogin = true;
                user.rememberToken = token;
                user.tokenExpireAt = expireAt;
                await mConnection.UpdateAsync(user);
            }
        }

        /// <summary>
        /// 通过Token自动登录
        /// </summary>
        /// <param name="token">记住登录的Token</param>
        /// <returns>对应用户，Token无效或过期返回null</returns>
        public async Task<User> AutoLoginByTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            var user = await mConnection.Table<User>()
                .Where(u => u.rememberToken == token)
                .FirstOrDefaultAsync();

            // 验证Token有效性
            if (user == null) return null;
            if (user.tokenExpireAt == null || user.tokenExpireAt < DateTime.Now)
            {
                // Token过期，清除
                user.rememberToken = null;
                user.rememberLogin = false;
                await mConnection.UpdateAsync(user);
                return null;
            }
            return user;
        }

        /// <summary>
        /// 清除记住登录状态（退出登录时调用）
        /// </summary>
        public async Task ClearRememberTokenAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                user.rememberToken = null;
                user.rememberLogin = false;
                user.tokenExpireAt = null;
                await mConnection.UpdateAsync(user);
            }
        }
    }
}