using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// 导航索引转可见性转换器
    /// 当 SelectedNavIndex 等于 ConverterParameter 时显示，否则隐藏
    /// ConverterParameter 传入目标页面索引（如 0, 1, 2...）
    /// </summary>
    public class NavIndexToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && parameter is string paramStr && int.TryParse(paramStr, out int targetIndex))
            {
                return selectedIndex == targetIndex ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
