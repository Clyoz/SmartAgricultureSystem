using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using System;
using System.Threading.Tasks;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// 认证授权服务
    /// 负责登录验证、权限判断、会话管理
    /// </summary>
    public class AuthService
    {
        // 用户服务依赖
        private readonly UserService mUserService;

        /// <summary>
        /// 当前已登录的用户（null表示未登录）
        /// </summary>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLoggedIn => CurrentUser != null;

        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin => CurrentUser?.role == UserRole.Admin;

        /// <summary>
        /// 用户登出事件
        /// </summary>
        public event Action OnLogout;

        /// <summary>
        /// 用户登录成功事件
        /// </summary>
        public event Action<User> OnLoginSuccess;

        public AuthService(UserService userService)
        {
            mUserService = userService;
        }
        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="rememberMe">是否记住登录</param>
        /// <returns>登录结果：(是否成功, 错误消息)</returns>
        public async Task<(bool success, string message)> LoginAsync(
            string username, string password, bool rememberMe = false)
        {

            // 基础参数验证
            if (string.IsNullOrWhiteSpace(username))
                return (false, "用户名不能为空");
            if (string.IsNullOrWhiteSpace(password))
                return (false, "密码不能为空");

            // 查询用户
            var user = await mUserService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                await mUserService.RecordLoginAsync(username, false, "用户不存在");
                return (false, "用户名或密码错误");
            }

            // 检查账号是否被锁定
            if (user.isLocked)
            {
                // 检查是否到了解锁时间
                if (user.lockUntil.HasValue && user.lockUntil.Value <= DateTime.Now)
                {
                    await mUserService.UnlockUserAsync(user.id);
                }
                else
                {
                    string lockMsg = user.lockUntil == DateTime.MaxValue
                        ? "账号已被永久锁定，请联系管理员"
                        : $"账号已被锁定，请于 {user.lockUntil:HH:mm:ss} 后重试";
                    await mUserService.RecordLoginAsync(username, false, "账号已锁定");
                    return (false, lockMsg);
                }
            }

            // 验证密码
            bool pwdMatch = PasswordHelper.VerifyPassword(
                password, user.passwordHash, user.passwordSalt);

            if (!pwdMatch)
            {
                // 记录失败次数
                await mUserService.HandleLoginFailAsync(user);
                int remaining = 5 - user.failedLoginCount;
                string failMsg = remaining > 0
                    ? $"密码错误，还剩 {remaining} 次机会"
                    : "密码错误次数过多，账号已被锁定30分钟";
                await mUserService.RecordLoginAsync(username, false, "密码错误");
                return (false, failMsg);
            }

            // 登录成功
            await mUserService.ResetLoginFailAsync(user);
            await mUserService.RecordLoginAsync(username, true);

            // 处理"记住登录"
            if (rememberMe)
            {
                string token = Guid.NewGuid().ToString("N");
                DateTime expireAt = DateTime.Now.AddDays(7); // 7天有效期
                await mUserService.SaveRememberTokenAsync(user.id, token, expireAt);
                // 保存Token到本地设置
                SessionService.SaveRememberToken(token);
            }

            CurrentUser = user;
            OnLoginSuccess?.Invoke(user);
            return (true, "登录成功");
        }
        /// <summary>
        /// 通过记住登录Token自动登录
        /// </summary>
        /// <returns>是否自动登录成功</returns>
        public async Task<bool> AutoLoginAsync()
        {
            string token = SessionService.LoadRememberToken();
            if (string.IsNullOrEmpty(token)) return false;

            var user = await mUserService.AutoLoginByTokenAsync(token);
            if (user == null)
            {
                SessionService.ClearRememberToken();
                return false;
            }

            // 检查账号锁定状态
            if (user.isLocked)
            {
                SessionService.ClearRememberToken();
                return false;
            }

            CurrentUser = user;
            OnLoginSuccess?.Invoke(user);
            return true;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public async Task LogoutAsync()
        {
            if (CurrentUser != null)
            {
                // 清除记住登录Token
                await mUserService.ClearRememberTokenAsync(CurrentUser.id);
                SessionService.ClearRememberToken();
                CurrentUser = null;
                OnLogout?.Invoke();
            }
        }

        /// <summary>
        /// 检查当前用户是否有权限访问指定设备
        /// 管理员可访问所有设备，农户只能访问绑定的设备
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>是否有权限</returns>
        public bool HasDevicePermission(int deviceId)
        {
            if (!IsLoggedIn) return false;
            if (IsAdmin) return true; // 管理员有所有权限

            // TODO: 通过GreenhouseCrop表查询农户关联的大棚设备权限
            // 当前简化实现：农户默认可访问所有设备
            return true;
        }
    }
}