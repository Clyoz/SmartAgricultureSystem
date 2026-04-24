using SmartAgricultureSystem.Models;
using System;
using System.Media;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// 预警服务
    /// 负责判断传感器数据是否超出阈值并触发预警
    /// </summary>
    public class AlertService
    {
        // 预警配置
        private readonly AlertConfig mConfig;

        /// <summary>
        /// 预警事件，当触发预警时通知订阅者
        /// 参数：预警消息内容
        /// </summary>
        public event Action<string> OnAlert;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">预警配置</param>
        public AlertService(AlertConfig config)
        {
            mConfig = config;
        }

        /// <summary>
        /// 检查传感器数据是否触发预警
        /// </summary>
        /// <param name="data">待检查的传感器数据</param>
        /// <returns>是否触发了预警</returns>
        public bool CheckAndAlert(SensorData data)
        {
            bool triggered = false;
            string alertMessage = string.Empty;

            // 检查温度上限
            if (data.temperature > mConfig.TEMP_MAX)
            {
                alertMessage += $"⚠️ 温度过高！当前: {data.temperature:F1}℃ > 上限: {mConfig.TEMP_MAX}℃\n";
                triggered = true;
            }
            // 检查温度下限
            else if (data.temperature < mConfig.TEMP_MIN)
            {
                alertMessage += $"⚠️ 温度过低！当前: {data.temperature:F1}℃ < 下限: {mConfig.TEMP_MIN}℃\n";
                triggered = true;
            }

            // 检查湿度上限
            if (data.humidity > mConfig.HUMIDITY_MAX)
            {
                alertMessage += $"⚠️ 湿度过高！当前: {data.humidity:F1}% > 上限: {mConfig.HUMIDITY_MAX}%\n";
                triggered = true;
            }
            // 检查湿度下限
            else if (data.humidity < mConfig.HUMIDITY_MIN)
            {
                alertMessage += $"⚠️ 湿度过低！当前: {data.humidity:F1}% < 下限: {mConfig.HUMIDITY_MIN}%\n";
                triggered = true;
            }

            if (triggered)
            {
                data.isAlert = true;
                // 触发预警事件
                OnAlert?.Invoke(alertMessage.TrimEnd());
                // 播放系统警告音
                SystemSounds.Exclamation.Play();
            }

            return triggered;
        }
    }
}