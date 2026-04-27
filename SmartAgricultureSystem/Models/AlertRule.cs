using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 预警规则模型
    /// 定义传感器数据的预警阈值
    /// </summary>
    public class AlertRule
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>规则名称</summary>
        public string ruleName { get; set; }

        /// <summary>关联传感器类型：1=温度, 2=湿度, 3=光照, 4=土壤湿度, 5=CO2浓度</summary>
        public int sensorType { get; set; }

        /// <summary>适用大棚ID（null表示全局规则）</summary>
        public int? greenhouseId { get; set; }

        /// <summary>预警下限值</summary>
        public double? minValue { get; set; }

        /// <summary>预警上限值</summary>
        public double? maxValue { get; set; }

        /// <summary>预警级别：1=提示, 2=警告, 3=严重</summary>
        public int alertLevel { get; set; } = 2;

        /// <summary>是否启用</summary>
        public bool isEnabled { get; set; } = true;

        /// <summary>备注</summary>
        public string remark { get; set; }

        /// <summary>创建时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;

        /// <summary>更新时间</summary>
        public DateTime? updatedAt { get; set; }
    }
}
