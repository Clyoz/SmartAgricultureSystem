using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartAgricultureSystem.Views
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private UserManagementViewModel ViewModel => DataContext as UserManagementViewModel;

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                var dialog = new UserEditDialog(user);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    ViewModel?.RefreshCommand.Execute(null);
                }
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                if (user.username == "admin")
                {
                    MessageBox.Show("无法删除超级管理员账号！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = MessageBox.Show($"确定要删除用户 \"{user.username}\" 吗？", "确认删除",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ViewModel?.DeleteUser(user.id);
                }
            }
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                var dialog = new ResetPasswordDialog(user);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
            }
        }

        private void BtnUnlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                if (!user.isLocked)
                {
                    MessageBox.Show("该用户未被锁定", "提示");
                    return;
                }
                ViewModel?.UnlockUser(user.id);
            }
        }
    }
}
