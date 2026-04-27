using SmartAgricultureSystem.ViewModels;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    /// <summary>
    /// ProfileWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private ProfileViewModel mViewModel;

        public ProfileWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 修改密码按钮点击事件
        /// 从PasswordBox获取密码后传给ViewModel
        /// </summary>
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (mViewModel != null)
            {
                mViewModel.OldPassword = PwdOld.Password;
                mViewModel.NewPassword = PwdNew.Password;
                mViewModel.ConfirmPassword = PwdConfirm.Password;

                if (mViewModel.ChangePasswordCommand.CanExecute(null))
                    mViewModel.ChangePasswordCommand.Execute(null);
            }
        }

        /// <summary>
        /// 返回主界面按钮点击事件
        /// 关闭个人信息窗口，返回主界面
        /// </summary>
        private void BtnGoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 设置ViewModel（由ServiceLocator调用）
        /// </summary>
        public void SetViewModel(ProfileViewModel viewModel)
        {
            mViewModel = viewModel;
            DataContext = mViewModel;
        }
    }
}
