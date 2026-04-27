using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 传感器采集数据模型
    /// 存储传感器实时采集的温湿度等数据
    /// </summary>
    public class SensorData
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>传感器ID（关联Sensors表）</summary>
        public int sensorId { get; set; }

        /// <summary>设备ID（关联Devices表，冗余字段便于查询）</summary>
        public int deviceId { get; set; }

        /// <summary>大棚ID（关联Greenhouses表，冗余字段便于查询）</summary>
        public int greenhouseId { get; set; }

        /// <summary>温度值（摄氏度）</summary>
        public double temperature { get; set; }

        /// <summary>湿度值（百分比）</summary>
        public double humidity { get; set; }

        /// <summary>光照强度（Lux）</summary>
        public double? lightIntensity { get; set; }

        /// <summary>土壤湿度（百分比）</summary>
        public double? soilMoisture { get; set; }

        /// <summary>CO2浓度（ppm）</summary>
        public double? co2Concentration { get; set; }

        /// <summary>土壤温度（摄氏度）</summary>
        public double? soilTemperature { get; set; }

        /// <summary>数据采集时间戳</summary>
        public DateTime timestamp { get; set; } = DateTime.Now;

        /// <summary>是否触发预警</summary>
        public bool isAlert { get; set; }
    }
}
