using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// 字符串转可见性转换器
    /// 非空非空白字符串 => Visible，否则 => Collapsed
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
