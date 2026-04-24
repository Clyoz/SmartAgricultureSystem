using SmartAgricultureSystem.ViewModels;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    /// <summary>
    /// 登录窗口后台代码
    /// 负责处理PasswordBox（WPF安全控件，不支持直接绑定）
    /// </summary>
    public partial class LoginWindow : Window
    {
        // 登录视图模型
        private LoginViewModel mViewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            mViewModel = viewModel;
            DataContext = mViewModel;

            // 设置登录成功回调
            mViewModel.OnLoginSuccessCallback = () => {
                // 打开主窗口
                var mainWindow = App.ServiceLocator.GetMainWindow();
                mainWindow.Show();
                this.Close();
            };

            // 设置跳转注册回调
            mViewModel.OnGoRegisterCallback = () => {
                var registerWindow = App.ServiceLocator.GetRegisterWindow();
                registerWindow.Show();
                this.Close();
            };
        }

        /// <summary>
        /// 登录按钮点击事件
        /// 从PasswordBox获取密码后传给ViewModel
        /// </summary>
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 从PasswordBox获取密码（不绑定到ViewModel，保证安全）
            await mViewModel.ExecuteLoginAsync(PwdBox.Password);
        }
    }
}