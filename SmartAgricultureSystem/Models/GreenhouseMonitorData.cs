using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 大棚监控数据模型（用于数据监控页面展示）
    /// 包含大棚基本信息、当前温湿度、作物阈值及预警状态
    /// </summary>
    public class GreenhouseMonitorData : INotifyPropertyChanged
    {
        /// <summary>大棚ID</summary>
        public int greenhouseId { get; set; }

        /// <summary>大棚名称</summary>
        public string greenhouseName { get; set; }

        /// <summary>作物名称</summary>
        public string cropName { get; set; }

        /// <summary>温度下限阈值</summary>
        public double tempMin { get; set; }

        /// <summary>温度上限阈值</summary>
        public double tempMax { get; set; }

        /// <summary>湿度下限阈值</summary>
        public double humidityMin { get; set; }

        /// <summary>湿度上限阈值</summary>
        public double humidityMax { get; set; }

        private double mCurrentTemperature;
        /// <summary>当前温度</summary>
        public double CurrentTemperature
        {
            get => mCurrentTemperature;
            set { mCurrentTemperature = value; OnPropertyChanged(); OnPropertyChanged(nameof(TempStatus)); }
        }

        private double mCurrentHumidity;
        /// <summary>当前湿度</summary>
        public double CurrentHumidity
        {
            get => mCurrentHumidity;
            set { mCurrentHumidity = value; OnPropertyChanged(); OnPropertyChanged(nameof(HumidityStatus)); }
        }

        private double mTargetTemperature;
        /// <summary>目标温度（调节器设定值）</summary>
        public double TargetTemperature
        {
            get => mTargetTemperature;
            set { mTargetTemperature = value; OnPropertyChanged(); }
        }

        private double mTargetHumidity;
        /// <summary>目标湿度（调节器设定值）</summary>
        public double TargetHumidity
        {
            get => mTargetHumidity;
            set { mTargetHumidity = value; OnPropertyChanged(); }
        }

        private bool mIsStabilized;
        /// <summary>数据是否已稳定（初始上升完成后才触发预警）</summary>
        public bool IsStabilized
        {
            get => mIsStabilized;
            set { mIsStabilized = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 温度预警状态：0=正常(绿), 1=接近阈值(黄), 2=超限(红)
        /// 接近阈值定义：距阈值20%范围内
        /// </summary>
        public int TempStatus
        {
            get
            {
                if (CurrentTemperature == 0) return 0;
                if (CurrentTemperature <= tempMin || CurrentTemperature >= tempMax)
                    return 2; // 超限-红
                double tempRange = tempMax - tempMin;
                double warnMargin = tempRange * 0.2; // 20%为接近阈值
                if (CurrentTemperature <= tempMin + warnMargin || CurrentTemperature >= tempMax - warnMargin)
                    return 1; // 接近阈值-黄
                return 0; // 正常-绿
            }
        }

        /// <summary>
        /// 湿度预警状态：0=正常(绿), 1=接近阈值(黄), 2=超限(红)
        /// </summary>
        public int HumidityStatus
        {
            get
            {
                if (CurrentHumidity == 0) return 0;
                if (CurrentHumidity <= humidityMin || CurrentHumidity >= humidityMax)
                    return 2; // 超限-红
                double humRange = humidityMax - humidityMin;
                double warnMargin = humRange * 0.2;
                if (CurrentHumidity <= humidityMin + warnMargin || CurrentHumidity >= humidityMax - warnMargin)
                    return 1; // 接近阈值-黄
                return 0; // 正常-绿
            }
        }

        /// <summary>最后一次更新时间</summary>
        public DateTime? LastUpdateTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
