using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class ResetPasswordDialog : Window
    {
        private readonly UserService mUserService;
        private readonly User mUser;

        public ResetPasswordDialog(User user)
        {
            InitializeComponent();
            mUserService = new UserService();
            mUser = user;
            RunUsername.Text = user.username;
        }

        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewPassword.Password))
            {
                MessageBox.Show("请输入新密码", "提示");
                return;
            }
            if (TxtNewPassword.Password != TxtConfirmPassword.Password)
            {
                MessageBox.Show("两次输入的密码不一致", "提示");
                return;
            }

            try
            {
                await mUserService.ResetPasswordAsync(mUser.id, TxtNewPassword.Password);
                MessageBox.Show("密码重置成功", "提示");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重置失败: {ex.Message}", "错误");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
