namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 预警阈值配置模型
    /// </summary>
    public class AlertConfig
    {
        /// <summary>
        /// 温度上限（摄氏度）
        /// </summary>
        public double TEMP_MAX { get; set; } = 35.0;

        /// <summary>
        /// 温度下限（摄氏度）
        /// </summary>
        public double TEMP_MIN { get; set; } = 5.0;

        /// <summary>
        /// 湿度上限（百分比）
        /// </summary>
        public double HUMIDITY_MAX { get; set; } = 90.0;

        /// <summary>
        /// 湿度下限（百分比）
        /// </summary>
        public double HUMIDITY_MIN { get; set; } = 20.0;

        /// <summary>
        /// 采集间隔（毫秒）
        /// </summary>
        public int POLL_INTERVAL_MS { get; set; } = 2000;
    }
}