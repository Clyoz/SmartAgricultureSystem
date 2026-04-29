using SmartAgricultureSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class LoginLogsWindow : Window
    {
        private readonly UserService mUserService;

        public LoginLogsWindow()
        {
            InitializeComponent();
            mUserService = new UserService();
            LoadLogs();
        }

        private async void LoadLogs()
        {
            try
            {
                var logs = await mUserService.GetRecentLoginRecordsAsync(100);
                var list = new ObservableCollection<Models.LoginRecord>(logs);
                DgLogs.ItemsSource = list;
                TxtStatus.Text = $"共 {logs.Count} 条记录";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载日志失败: {ex.Message}", "错误");
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }
    }
}
