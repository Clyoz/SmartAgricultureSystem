using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 设备模型
    /// 代表大棚中部署的硬件设备（传感器网关、控制器等）
    /// </summary>
    public class Device
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>设备编号（唯一，如DEV-001）</summary>
        public string deviceCode { get; set; }

        /// <summary>设备名称</summary>
        public string name { get; set; }

        /// <summary>所属大棚ID（关联Greenhouses表）</summary>
        public int greenhouseId { get; set; }

        /// <summary>所属大棚名称（仅用于显示，非数据库字段）</summary>
        public string greenhouseName { get; set; }

        /// <summary>设备类型：1=网关, 2=传感器节点, 3=控制器</summary>
        public int deviceType { get; set; }

        /// <summary>设备IP地址</summary>
        public string ipAddress { get; set; }

        /// <summary>Modbus端口</summary>
        public int port { get; set; } = 502;

        /// <summary>Modbus从站地址</summary>
        public byte slaveId { get; set; } = 1;

        /// <summary>设备型号</summary>
        public string model { get; set; }

        /// <summary>固件版本</summary>
        public string firmwareVersion { get; set; }

        /// <summary>在线状态：0=离线, 1=在线</summary>
        public bool isOnline { get; set; } = false;

        /// <summary>最后在线时间</summary>
        public DateTime? lastOnlineTime { get; set; }

        /// <summary>安装日期</summary>
        public DateTime? installDate { get; set; }

        /// <summary>备注</summary>
        public string remark { get; set; }

        /// <summary>创建时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;

        /// <summary>更新时间</summary>
        public DateTime? updatedAt { get; set; }
    }
}
