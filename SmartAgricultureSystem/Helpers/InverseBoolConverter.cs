using System;
using System.Globalization;
using System.Windows.Data;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// 布尔值取反转换器
    /// 将 true 转为 false，false 转为 true
    /// 用于绑定 IsEnabled 等属性时反转逻辑
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }
    }
}
