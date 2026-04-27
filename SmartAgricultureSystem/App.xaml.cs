using SmartAgricultureSystem.Services;
using SmartAgricultureSystem.ViewModels;
using SmartAgricultureSystem.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SmartAgricultureSystem
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        // 全局服务定位器，负责创建和管理窗口及依赖服务
        public static ServiceLocator ServiceLocator { get; } = new ServiceLocator();

        /// <summary>
        /// 应用启动时由 App.xaml 的 Startup 事件触发
        /// 使用 ServiceLocator 创建登录窗口（需要带参构造函数）
        /// </summary>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var loginWindow = ServiceLocator.GetLoginWindow();
            loginWindow.Show();
        }
    }

    /// <summary>
    /// 简易服务定位器
    /// 负责创建服务和ViewModel的依赖链，以及创建窗口实例
    /// </summary>
    public class ServiceLocator
    {
        // 懒加载的服务实例
        private UserService mUserService;
        private AuthService mAuthService;
        private DatabaseService mDatabaseService;

        public ServiceLocator()
        {
            // 初始化数据库和用户服务
            mDatabaseService = new DatabaseService();
            mUserService = new UserService();
            mAuthService = new AuthService(mUserService);

            // 异步初始化数据库表
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            // 数据库和表需提前在 SQL Server 中通过 Scripts/InitDatabase.sql 创建
            // 此处仅确保默认管理员账号存在
            await mUserService.InitializeAsync();
        }

        /// <summary>
        /// 获取登录窗口
        /// </summary>
        public LoginWindow GetLoginWindow()
        {
            var viewModel = new LoginViewModel(mAuthService);
            return new LoginWindow(viewModel);
        }

        /// <summary>
        /// 获取注册窗口（复用LoginWindow作为注册页）
        /// </summary>
        public LoginWindow GetRegisterWindow()
        {
            var viewModel = new LoginViewModel(mAuthService);
            return new LoginWindow(viewModel);
        }

        /// <summary>
        /// 获取主窗口
        /// </summary>
        public MainWindow GetMainWindow()
        {
            var viewModel = new MainViewModel();

            // 设置当前登录用户信息
            if (mAuthService?.CurrentUser != null)
            {
                viewModel.SetCurrentUser(mAuthService.CurrentUser.username);
                viewModel.SetAuthService(mAuthService);
            }

            var window = new MainWindow();
            window.SetViewModel(viewModel);

            // 设置退出登录回调
            viewModel.OnLogoutCallback = () =>
            {
                var loginWindow = GetLoginWindow();
                loginWindow.Show();
                window.Close();
            };

            // 设置打开个人中心回调
            viewModel.OnOpenProfileCallback = () =>
            {
                var profileWindow = GetProfileWindow(window);
                profileWindow.Owner = window;
                profileWindow.ShowDialog();
            };

            return window;
        }

        /// <summary>
        /// 获取个人信息窗口
        /// </summary>
        public ProfileWindow GetProfileWindow(Window mainWindow = null)
        {
            var viewModel = new ProfileViewModel(mUserService, mAuthService);
            var window = new ProfileWindow();
            window.SetViewModel(viewModel);

            // 设置退出登录回调：退出登录后回到登录页面
            viewModel.OnLogoutCallback = () =>
            {
                window.Close();
                if (mainWindow != null)
                {
                    mainWindow.Close();
                }
                var loginWindow = GetLoginWindow();
                loginWindow.Show();
            };

            return window;
        }
    }
}
