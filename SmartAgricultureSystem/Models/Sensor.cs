using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 传感器模型
    /// 代表具体的传感器探头（温度、湿度、光照、土壤等）
    /// </summary>
    public class Sensor
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>传感器编号（唯一，如SEN-T001）</summary>
        public string sensorCode { get; set; }

        /// <summary>传感器名称</summary>
        public string name { get; set; }

        /// <summary>所属设备ID（关联Devices表）</summary>
        public int deviceId { get; set; }

        /// <summary>传感器类型：1=温度, 2=湿度, 3=光照, 4=土壤湿度, 5=CO2浓度, 6=土壤温度</summary>
        public int sensorType { get; set; }

        /// <summary>Modbus寄存器起始地址</summary>
        public ushort registerAddress { get; set; }

        /// <summary>寄存器数量</summary>
        public int registerCount { get; set; } = 1;

        /// <summary>数据单位</summary>
        public string unit { get; set; }

        /// <summary>最小量程</summary>
        public double minValue { get; set; }

        /// <summary>最大量程</summary>
        public double maxValue { get; set; }

        /// <summary>精度系数（寄存器值 × 精度系数 = 实际值，如0.1）</summary>
        public double precisionFactor { get; set; } = 0.1;

        /// <summary>采集间隔（毫秒）</summary>
        public int pollIntervalMs { get; set; } = 2000;

        /// <summary>是否启用</summary>
        public bool isEnabled { get; set; } = true;

        /// <summary>备注</summary>
        public string remark { get; set; }

        /// <summary>创建时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;
    }
}
