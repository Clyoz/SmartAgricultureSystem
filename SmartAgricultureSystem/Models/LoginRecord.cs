using SQLite;
using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 登录记录模型
    /// 用于安全审计，记录每次登录的时间、IP、结果
    /// </summary>
    [Table("LoginRecords")]
    public class LoginRecord
    {
        /// <summary>主键，自增ID</summary>
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        /// <summary>用户名</summary>
        public string username { get; set; }

        /// <summary>登录时间</summary>
        public DateTime loginTime { get; set; } = DateTime.Now;

        /// <summary>登录是否成功</summary>
        public bool isSuccess { get; set; }

        /// <summary>失败原因（成功时为空）</summary>
        public string failReason { get; set; }

        /// <summary>登录设备信息（模拟）</summary>
        public string deviceInfo { get; set; } = "Windows PC";
    }
}