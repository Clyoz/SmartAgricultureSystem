using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartAgricultureSystem.Views
{
    public partial class DeviceManagementView : UserControl
    {
        public DeviceManagementView()
        {
            InitializeComponent();
        }

        private DeviceManagementViewModel ViewModel => DataContext as DeviceManagementViewModel;

        private void BtnEditDevice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Device device)
            {
                var dialog = new DeviceEditDialog(device);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    ViewModel?.RefreshCommand.Execute(null);
                }
            }
        }

        private void BtnDeleteDevice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Device device)
            {
                var result = MessageBox.Show($"确定要删除设备 \"{device.name}\" 吗？", "确认删除",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ViewModel?.DeleteDevice(device.id);
                }
            }
        }
    }
}
