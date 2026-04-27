using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 预警记录模型
    /// 记录每次预警的详细信息
    /// </summary>
    public class AlertRecord
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>关联预警规则ID</summary>
        public int? ruleId { get; set; }

        /// <summary>关联传感器数据ID</summary>
        public int? sensorDataId { get; set; }

        /// <summary>大棚ID</summary>
        public int greenhouseId { get; set; }

        /// <summary>设备ID</summary>
        public int deviceId { get; set; }

        /// <summary>传感器ID</summary>
        public int sensorId { get; set; }

        /// <summary>预警类型：1=温度过高, 2=温度过低, 3=湿度过高, 4=湿度过低, 5=光照异常, 6=CO2异常</summary>
        public int alertType { get; set; }

        /// <summary>预警级别：1=提示, 2=警告, 3=严重</summary>
        public int alertLevel { get; set; }

        /// <summary>触发值</summary>
        public double triggerValue { get; set; }

        /// <summary>阈值</summary>
        public double thresholdValue { get; set; }

        /// <summary>预警消息</summary>
        public string message { get; set; }

        /// <summary>处理状态：0=未处理, 1=处理中, 2=已处理</summary>
        public int handleStatus { get; set; } = 0;

        /// <summary>处理人ID</summary>
        public int? handlerId { get; set; }

        /// <summary>处理时间</summary>
        public DateTime? handleTime { get; set; }

        /// <summary>处理备注</summary>
        public string handleRemark { get; set; }

        /// <summary>预警触发时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;
    }
}
