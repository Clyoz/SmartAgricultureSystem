using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 系统配置模型
    /// 存储系统级参数配置（键值对形式）
    /// </summary>
    public class SystemConfig
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>配置键（唯一）</summary>
        public string configKey { get; set; }

        /// <summary>配置值</summary>
        public string configValue { get; set; }

        /// <summary>配置分组（如System、Alert、Modbus等）</summary>
        public string configGroup { get; set; }

        /// <summary>配置描述</summary>
        public string description { get; set; }

        /// <summary>是否启用</summary>
        public bool isEnabled { get; set; } = true;

        /// <summary>创建时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;

        /// <summary>更新时间</summary>
        public DateTime? updatedAt { get; set; }
    }
}
