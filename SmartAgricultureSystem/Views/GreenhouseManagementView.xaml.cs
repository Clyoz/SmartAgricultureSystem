using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartAgricultureSystem.Views
{
    public partial class GreenhouseManagementView : UserControl
    {
        public GreenhouseManagementView()
        {
            InitializeComponent();
        }

        private GreenhouseManagementViewModel ViewModel => DataContext as GreenhouseManagementViewModel;

        private void BtnEditGreenhouse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Greenhouse gh)
            {
                var dialog = new GreenhouseEditDialog(gh);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    ViewModel?.RefreshCommand.Execute(null);
                }
            }
        }

        private void BtnDeleteGreenhouse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Greenhouse gh)
            {
                var result = MessageBox.Show($"确定要删除大棚 \"{gh.name}\" 吗？", "确认删除",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ViewModel?.DeleteGreenhouse(gh.id);
                }
            }
        }
    }
}
