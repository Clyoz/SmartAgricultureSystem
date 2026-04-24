using SQLite;
using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 传感器数据模型，用于存储温湿度采集数据
    /// </summary>
    [Table("SensorData")]
    public class SensorData
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string deviceId { get; set; }

        /// <summary>
        /// 温度值（摄氏度）
        /// </summary>
        public double temperature { get; set; }

        /// <summary>
        /// 湿度值（百分比）
        /// </summary>
        public double humidity { get; set; }

        /// <summary>
        /// 数据采集时间戳
        /// </summary>
        public DateTime timestamp { get; set; }

        /// <summary>
        /// 是否触发预警
        /// </summary>
        public bool isAlert { get; set; }
    }
}