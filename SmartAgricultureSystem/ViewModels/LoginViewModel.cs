using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SmartAgricultureSystem.ViewModels
{
    /// <summary>
    /// 登录界面视图模型
    /// 处理账号密码登录、验证码校验、记住登录状态
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        // 认证服务
        private readonly AuthService mAuthService;

        // 当前验证码字符串（用于校验）
        private string mCurrentCaptchaCode;

        // 用户名
        private string mUsername;

        // 验证码输入
        private string mInputCaptcha;

        // 是否记住登录
        private bool mRememberMe;

        // 错误提示信息
        private string mErrorMessage;

        // 是否正在登录（防止重复点击）
        private bool mIsLoading;

        // 验证码图片
        private BitmapImage mCaptchaImage;
        #region 属性绑定

        public string Username
        {
            get => mUsername;
            set { mUsername = value; OnPropertyChanged(); }
        }

        public string InputCaptcha
        {
            get => mInputCaptcha;
            set { mInputCaptcha = value; OnPropertyChanged(); }
        }

        public bool RememberMe
        {
            get => mRememberMe;
            set { mRememberMe = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => mErrorMessage;
            set { mErrorMessage = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => mIsLoading;
            set { mIsLoading = value; OnPropertyChanged(); }
        }

        public BitmapImage CaptchaImage
        {
            get => mCaptchaImage;
            set { mCaptchaImage = value; OnPropertyChanged(); }
        }

        #endregion
        #region 命令

        /// <summary>登录命令</summary>
        public ICommand LoginCommand { get; }

        /// <summary>刷新验证码命令</summary>
        public ICommand RefreshCaptchaCommand { get; }

        /// <summary>跳转注册命令</summary>
        public ICommand GoRegisterCommand { get; }

        #endregion

        /// <summary>
        /// 登录成功回调（通知View切换界面）
        /// </summary>
        public System.Action OnLoginSuccessCallback { get; set; }

        /// <summary>
        /// 跳转注册回调
        /// </summary>
        public System.Action OnGoRegisterCallback { get; set; }

        public LoginViewModel(AuthService authService)
        {
            mAuthService = authService;

            LoginCommand = new RelayCommand(
                async _ => await ExecuteLoginAsync(),
                _ => !IsLoading);
            RefreshCaptchaCommand = new RelayCommand(_ => RefreshCaptcha());
            GoRegisterCommand = new RelayCommand(_ => OnGoRegisterCallback?.Invoke());

            // 初始化验证码
            RefreshCaptcha();
        }

        /// <summary>
        /// 刷新验证码
        /// </summary>
        public void RefreshCaptcha()
        {
            mCurrentCaptchaCode = CaptchaHelper.GenerateCaptchaCode();
            CaptchaImage = CaptchaHelper.GenerateCaptchaImage(mCurrentCaptchaCode);
            InputCaptcha = string.Empty;
        }

        /// <summary>
        /// 执行登录逻辑
        /// </summary>
        /// <param name="password">密码（从View传入，避免ViewModel持有明文密码）</param>
        public async Task ExecuteLoginAsync(string password = null)
        {
            ErrorMessage = string.Empty;

            // 验证验证码（不区分大小写）
            if (string.IsNullOrWhiteSpace(InputCaptcha) ||
                !InputCaptcha.Equals(mCurrentCaptchaCode,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "验证码错误，请重新输入";
                RefreshCaptcha();
                return;
            }

            IsLoading = true;
            var (success, message) = await mAuthService.LoginAsync(
                Username, password ?? string.Empty, RememberMe);
            IsLoading = false;

            if (success)
            {
                OnLoginSuccessCallback?.Invoke();
            }
            else
            {
                ErrorMessage = message;
                RefreshCaptcha(); // 登录失败刷新验证码
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}