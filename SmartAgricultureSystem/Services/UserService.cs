using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// 用户业务服务
    /// 使用 SQL Server + ADO.NET 实现用户注册、查询、信息修改、账号管理等操作
    /// </summary>
    public class UserService
    {
        // 数据库服务
        private readonly DatabaseService mDb;

        public UserService()
        {
            mDb = new DatabaseService();
        }

        /// <summary>
        /// 初始化（确保默认管理员存在）
        /// </summary>
        public async Task InitializeAsync()
        {
            await EnsureDefaultAdminAsync();
        }

        /// <summary>
        /// 创建默认管理员账号（admin / Admin@123456）
        /// </summary>
        private async Task EnsureDefaultAdminAsync()
        {
            var admin = await GetUserByUsernameAsync("admin");
            if (admin == null)
            {
                string salt = PasswordHelper.GenerateSalt();
                var user = new User
                {
                    username = "admin",
                    passwordHash = PasswordHelper.HashPassword("Admin@123456", salt),
                    passwordSalt = salt,
                    nickname = "系统管理员",
                    role = UserRole.Admin,
                    createdAt = DateTime.Now
                };
                await InsertUserAsync(user);
            }
        }

        /// <summary>
        /// 插入用户
        /// </summary>
        private async Task<int> InsertUserAsync(User user)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO Users (username, passwordHash, passwordSalt, nickname, phoneNumber, email,
    avatarPath, role, isLocked, failedLoginCount, lockUntil, createdAt, lastLoginAt,
    rememberLogin, rememberToken, tokenExpireAt, remark)
VALUES (@username, @passwordHash, @passwordSalt, @nickname, @phoneNumber, @email,
    @avatarPath, @role, @isLocked, @failedLoginCount, @lockUntil, @createdAt, @lastLoginAt,
    @rememberLogin, @rememberToken, @tokenExpireAt, @remark);
SELECT SCOPE_IDENTITY();";
                    AddUserParameters(cmd, user);
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// 注册新用户
        /// </summary>
        public async Task<string> RegisterAsync(string username, string password, string nickname, UserRole role = UserRole.Farmer)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]{4,20}$"))
                return "用户名为4-20位字母、数字或下划线";

            string pwdError = PasswordHelper.ValidatePasswordStrength(password);
            if (pwdError != null) return pwdError;

            var existing = await GetUserByUsernameAsync(username);
            if (existing != null) return "用户名已被注册";

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
            await InsertUserAsync(newUser);
            return null;
        }

        /// <summary>
        /// 根据用户名查询用户
        /// </summary>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return ReadUser(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 根据ID查询用户
        /// </summary>
        public async Task<User> GetUserByIdAsync(int userId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return ReadUser(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有用户列表
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            var result = new List<User>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users ORDER BY createdAt DESC";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            result.Add(ReadUser(reader));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 更新用户昵称
        /// </summary>
        public async Task UpdateNicknameAsync(int userId, string newNickname)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET nickname = @nickname WHERE id = @id";
                    cmd.Parameters.AddWithValue("@nickname", newNickname);
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 更新用户头像路径
        /// </summary>
        public async Task UpdateAvatarAsync(int userId, string avatarPath)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET avatarPath = @avatarPath WHERE id = @id";
                    cmd.Parameters.AddWithValue("@avatarPath", (object)avatarPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task<string> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return "用户不存在";

            if (!PasswordHelper.VerifyPassword(oldPassword, user.passwordHash, user.passwordSalt))
                return "原密码错误";

            string pwdError = PasswordHelper.ValidatePasswordStrength(newPassword);
            if (pwdError != null) return pwdError;

            if (PasswordHelper.VerifyPassword(newPassword, user.passwordHash, user.passwordSalt))
                return "新密码不能与原密码相同";

            string newSalt = PasswordHelper.GenerateSalt();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET passwordHash = @hash, passwordSalt = @salt WHERE id = @id";
                    cmd.Parameters.AddWithValue("@hash", PasswordHelper.HashPassword(newPassword, newSalt));
                    cmd.Parameters.AddWithValue("@salt", newSalt);
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return null;
        }

        /// <summary>
        /// 绑定手机号
        /// </summary>
        public async Task<string> BindPhoneAsync(int userId, string phone)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1[3-9]\d{9}$"))
                return "手机号格式不正确";

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE phoneNumber = @phone AND id != @id";
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@id", userId);
                    int count = (int)await cmd.ExecuteScalarAsync();
                    if (count > 0) return "该手机号已被其他账号绑定";
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET phoneNumber = @phone WHERE id = @id";
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return null;
        }

        /// <summary>
        /// 锁定账号
        /// </summary>
        public async Task LockUserAsync(int userId, int lockMinutes = -1)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Users SET isLocked = 1, lockUntil = @lockUntil WHERE id = @id";
                    cmd.Parameters.AddWithValue("@lockUntil",
                        lockMinutes == -1 ? (object)DateTime.MaxValue : DateTime.Now.AddMinutes(lockMinutes));
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 解锁账号
        /// </summary>
        public async Task UnlockUserAsync(int userId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Users SET isLocked = 0, lockUntil = NULL, failedLoginCount = 0 WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 记录登录日志
        /// </summary>
        public async Task RecordLoginAsync(string username, bool isSuccess, string failReason = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO LoginRecords (username, loginTime, isSuccess, failReason)
VALUES (@username, GETDATE(), @isSuccess, @failReason)";
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@isSuccess", isSuccess);
                    cmd.Parameters.AddWithValue("@failReason", (object)failReason ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 处理登录失败
        /// </summary>
        public async Task HandleLoginFailAsync(User user)
        {
            user.failedLoginCount++;
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    if (user.failedLoginCount >= 5)
                    {
                        cmd.CommandText = @"UPDATE Users SET failedLoginCount = @count, isLocked = 1, lockUntil = @lockUntil WHERE id = @id";
                        cmd.Parameters.AddWithValue("@lockUntil", DateTime.Now.AddMinutes(30));
                    }
                    else
                    {
                        cmd.CommandText = @"UPDATE Users SET failedLoginCount = @count WHERE id = @id";
                    }
                    cmd.Parameters.AddWithValue("@count", user.failedLoginCount);
                    cmd.Parameters.AddWithValue("@id", user.id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 重置登录失败次数
        /// </summary>
        public async Task ResetLoginFailAsync(User user)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Users SET failedLoginCount = 0, lastLoginAt = GETDATE() WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", user.id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 保存记住登录Token
        /// </summary>
        public async Task SaveRememberTokenAsync(int userId, string token, DateTime expireAt)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Users SET rememberLogin = 1, rememberToken = @token, tokenExpireAt = @expireAt WHERE id = @id";
                    cmd.Parameters.AddWithValue("@token", token);
                    cmd.Parameters.AddWithValue("@expireAt", expireAt);
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 通过Token自动登录
        /// </summary>
        public async Task<User> AutoLoginByTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users WHERE rememberToken = @token";
                    cmd.Parameters.AddWithValue("@token", token);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var user = ReadUser(reader);
                            if (user.tokenExpireAt == null || user.tokenExpireAt < DateTime.Now)
                            {
                                // Token过期
                                reader.Close();
                                await ClearRememberTokenAsync(user.id);
                                return null;
                            }
                            return user;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 清除记住登录状态
        /// </summary>
        public async Task ClearRememberTokenAsync(int userId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Users SET rememberLogin = 0, rememberToken = NULL, tokenExpireAt = NULL WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #region 辅助方法

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings["SmartAgricultureDB"]?.ConnectionString
                ?? "Data Source=localhost;Initial Catalog=SmartAgricultureDB;Integrated Security=True;");
        }

        private User ReadUser(SqlDataReader reader)
        {
            return new User
            {
                id = (int)reader["id"],
                username = reader["username"].ToString(),
                passwordHash = reader["passwordHash"].ToString(),
                passwordSalt = reader["passwordSalt"].ToString(),
                nickname = reader["nickname"]?.ToString(),
                phoneNumber = reader["phoneNumber"]?.ToString(),
                email = reader["email"]?.ToString(),
                avatarPath = reader["avatarPath"]?.ToString(),
                role = (UserRole)(int)reader["role"],
                isLocked = (bool)reader["isLocked"],
                failedLoginCount = (int)reader["failedLoginCount"],
                lockUntil = reader["lockUntil"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["lockUntil"],
                createdAt = (DateTime)reader["createdAt"],
                lastLoginAt = reader["lastLoginAt"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["lastLoginAt"],
                rememberLogin = reader["rememberLogin"] != DBNull.Value && (bool)reader["rememberLogin"],
                rememberToken = reader["rememberToken"]?.ToString(),
                tokenExpireAt = reader["tokenExpireAt"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["tokenExpireAt"],
                remark = reader["remark"]?.ToString()
            };
        }

        private void AddUserParameters(SqlCommand cmd, User user)
        {
            cmd.Parameters.AddWithValue("@username", user.username);
            cmd.Parameters.AddWithValue("@passwordHash", user.passwordHash);
            cmd.Parameters.AddWithValue("@passwordSalt", user.passwordSalt);
            cmd.Parameters.AddWithValue("@nickname", (object)user.nickname ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phoneNumber", (object)user.phoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object)user.email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@avatarPath", (object)user.avatarPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@role", (int)user.role);
            cmd.Parameters.AddWithValue("@isLocked", user.isLocked);
            cmd.Parameters.AddWithValue("@failedLoginCount", user.failedLoginCount);
            cmd.Parameters.AddWithValue("@lockUntil", (object)user.lockUntil ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@createdAt", user.createdAt);
            cmd.Parameters.AddWithValue("@lastLoginAt", (object)user.lastLoginAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rememberLogin", user.rememberLogin);
            cmd.Parameters.AddWithValue("@rememberToken", (object)user.rememberToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tokenExpireAt", (object)user.tokenExpireAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@remark", (object)user.remark ?? DBNull.Value);
        }

        #endregion
    }
}
