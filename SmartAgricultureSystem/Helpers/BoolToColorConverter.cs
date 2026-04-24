using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// 布尔值转颜色转换器
    /// true => 绿色（成功），false => 红色（失败）
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSuccess)
            {
                return isSuccess
                    ? Color.FromArgb(0xFF, 0x40, 0xA0, 0x2B)  // 绿色 #40A02B
                    : Color.FromArgb(0xFF, 0xD2, 0x0F, 0x39);  // 红色 #D20F39
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
