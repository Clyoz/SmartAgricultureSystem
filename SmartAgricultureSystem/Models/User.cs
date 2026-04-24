using SQLite;
using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 用户数据模型
    /// 存储用户账号、密码（哈希）、角色、状态等信息
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>主键，自增ID</summary>
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        /// <summary>用户名（唯一）</summary>
        [Unique, NotNull]
        public string username { get; set; }

        /// <summary>密码哈希值（SHA256 + Salt）</summary>
        [NotNull]
        public string passwordHash { get; set; }

        /// <summary>密码盐值</summary>
        [NotNull]
        public string passwordSalt { get; set; }

        /// <summary>用户昵称</summary>
        public string nickname { get; set; }

        /// <summary>绑定手机号（模拟）</summary>
        public string phoneNumber { get; set; }

        /// <summary>头像路径（本地文件路径或Base64）</summary>
        public string avatarPath { get; set; }

        /// <summary>用户角色</summary>
        public UserRole role { get; set; } = UserRole.Farmer;

        /// <summary>账号是否被锁定</summary>
        public bool isLocked { get; set; } = false;

        /// <summary>连续登录失败次数（超过5次锁定账号）</summary>
        public int failedLoginCount { get; set; } = 0;

        /// <summary>账号锁定到期时间（null表示未锁定）</summary>
        public DateTime? lockUntil { get; set; }

        /// <summary>注册时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;

        /// <summary>最后登录时间</summary>
        public DateTime? lastLoginAt { get; set; }

        /// <summary>绑定的大棚设备ID（农户专用，逗号分隔）</summary>
        public string bindDeviceIds { get; set; }

        /// <summary>是否记住登录状态</summary>
        public bool rememberLogin { get; set; } = false;

        /// <summary>记住登录的Token</summary>
        public string rememberToken { get; set; }

        /// <summary>Token过期时间</summary>
        public DateTime? tokenExpireAt { get; set; }
    }
}