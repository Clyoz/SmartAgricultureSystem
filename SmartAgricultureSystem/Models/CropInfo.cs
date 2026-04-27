using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 作物信息模型
    /// 记录种植作物的品种、适宜环境参数等
    /// </summary>
    public class CropInfo
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>作物名称</summary>
        public string cropName { get; set; }

        /// <summary>作物品种</summary>
        public string variety { get; set; }

        /// <summary>适宜温度下限（℃）</summary>
        public double tempMin { get; set; }

        /// <summary>适宜温度上限（℃）</summary>
        public double tempMax { get; set; }

        /// <summary>适宜湿度下限（%）</summary>
        public double humidityMin { get; set; }

        /// <summary>适宜湿度上限（%）</summary>
        public double humidityMax { get; set; }

        /// <summary>适宜光照下限（Lux）</summary>
        public double? lightMin { get; set; }

        /// <summary>适宜光照上限（Lux）</summary>
        public double? lightMax { get; set; }

        /// <summary>生长周期（天）</summary>
        public int growthCycleDays { get; set; }

        /// <summary>种植说明</summary>
        public string description { get; set; }

        /// <summary>创建时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;
    }
}
