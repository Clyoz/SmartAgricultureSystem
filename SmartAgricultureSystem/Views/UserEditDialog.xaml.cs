using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class UserEditDialog : Window
    {
        private readonly UserService mUserService;
        private readonly User mUser;
        private readonly bool mIsEdit;

        public UserEditDialog(User user)
        {
            InitializeComponent();
            mUserService = new UserService();

            if (user != null)
            {
                mIsEdit = true;
                mUser = user;
                Title = "编辑用户";
                PanelNewUser.Visibility = Visibility.Collapsed;

                TxtNickname.Text = user.nickname ?? "";
                CmbRole.SelectedIndex = (int)user.role - 1;
                if (CmbRole.SelectedIndex < 0) CmbRole.SelectedIndex = 1;
                ChkLocked.IsChecked = user.isLocked;
                TxtEmail.Text = user.email ?? "";
                TxtRemark.Text = user.remark ?? "";
            }
            else
            {
                mIsEdit = false;
                mUser = new User();
                Title = "添加用户";
                PanelNewUser.Visibility = Visibility.Visible;
                CmbRole.SelectedIndex = 1; // 默认农户
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (mIsEdit)
            {
                // 编辑现有用户
                UserRole role = (UserRole)(CmbRole.SelectedIndex + 1);
                bool isLocked = ChkLocked.IsChecked == true;

                try
                {
                    await mUserService.UpdateUserByAdminAsync(
                        mUser.id, role, isLocked,
                        TxtNickname.Text.Trim(),
                        TxtEmail.Text.Trim(),
                        TxtRemark.Text.Trim());
                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误");
                }
            }
            else
            {
                // 新增用户
                if (string.IsNullOrWhiteSpace(TxtUsername.Text))
                {
                    MessageBox.Show("请输入用户名", "提示");
                    return;
                }
                if (string.IsNullOrWhiteSpace(TxtPassword.Password))
                {
                    MessageBox.Show("请输入密码", "提示");
                    return;
                }

                UserRole role = (UserRole)(CmbRole.SelectedIndex + 1);
                string error = await mUserService.RegisterAsync(
                    TxtUsername.Text.Trim(),
                    TxtPassword.Password,
                    TxtNickname.Text.Trim(),
                    role);

                if (error != null)
                {
                    MessageBox.Show(error, "注册失败");
                    return;
                }

                // 注册成功后，如果有额外的锁定状态或邮箱要设置
                if (ChkLocked.IsChecked == true || !string.IsNullOrWhiteSpace(TxtEmail.Text.Trim()))
                {
                    var newUser = await mUserService.GetUserByUsernameAsync(TxtUsername.Text.Trim());
                    if (newUser != null)
                    {
                        await mUserService.UpdateUserByAdminAsync(
                            newUser.id, role, ChkLocked.IsChecked == true,
                            TxtNickname.Text.Trim(),
                            TxtEmail.Text.Trim(),
                            TxtRemark.Text.Trim());
                    }
                }

                DialogResult = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
