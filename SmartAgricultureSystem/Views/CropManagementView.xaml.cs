using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartAgricultureSystem.Views
{
    public partial class CropManagementView : UserControl
    {
        public CropManagementView()
        {
            InitializeComponent();
        }

        private CropManagementViewModel ViewModel => DataContext as CropManagementViewModel;

        private void BtnEditCrop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CropInfo crop)
            {
                var dialog = new CropEditDialog(crop);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    ViewModel?.RefreshCommand.Execute(null);
                }
            }
        }

        private void BtnDeleteCrop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CropInfo crop)
            {
                var result = MessageBox.Show($"确定要删除作物 \"{crop.cropName}\" 吗？", "确认删除",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ViewModel?.DeleteCrop(crop.id);
                }
            }
        }
    }
}
