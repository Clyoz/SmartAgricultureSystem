using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// 导航按钮背景色转换器
    /// true（当前页面）=> 活跃色, false => 透明
    /// </summary>
    public class NavBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive && isActive)
                return new SolidColorBrush(Color.FromArgb(0xFF, 0x31, 0x32, 0x44)); // #313244
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 导航按钮前景色转换器
    /// true（当前页面）=> 高亮色, false => 灰白色
    /// </summary>
    public class NavFgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive && isActive)
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xA6, 0xE3, 0xA1)); // #A6E3A1
            return new SolidColorBrush(Color.FromArgb(0xFF, 0xCD, 0xD6, 0xF4)); // #CDD6F4
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
