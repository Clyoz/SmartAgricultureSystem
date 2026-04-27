using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 控制指令模型
    /// 记录对设备的远程控制操作（通风、灌溉、遮阳等）
    /// </summary>
    public class ControlCommand
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>目标设备ID（关联Devices表）</summary>
        public int deviceId { get; set; }

        /// <summary>大棚ID</summary>
        public int greenhouseId { get; set; }

        /// <summary>指令类型：1=通风, 2=灌溉, 3=遮阳, 4=加温, 5=补光, 6=CO2补充</summary>
        public int commandType { get; set; }

        /// <summary>指令参数（如开度百分比、持续时间等）</summary>
        public string commandParam { get; set; }

        /// <summary>执行状态：0=待执行, 1=执行中, 2=已完成, 3=执行失败</summary>
        public int status { get; set; } = 0;

        /// <summary>下发用户ID</summary>
        public int userId { get; set; }

        /// <summary>下发方式：1=手动, 2=自动联动</summary>
        public int sendType { get; set; } = 1;

        /// <summary>执行结果消息</summary>
        public string resultMessage { get; set; }

        /// <summary>下发时间</summary>
        public DateTime sendTime { get; set; } = DateTime.Now;

        /// <summary>执行完成时间</summary>
        public DateTime? executeTime { get; set; }

        /// <summary>备注</summary>
        public string remark { get; set; }
    }
}
