using Microsoft.Win32;
using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartAgricultureSystem.ViewModels
{
    /// <summary>
    /// 个人信息管理视图模型
    /// 支持修改昵称、密码、头像、绑定手机号
    /// </summary>
    public class ProfileViewModel : INotifyPropertyChanged
    {
        // 用户服务
        private readonly UserService mUserService;

        // 认证服务（获取当前用户）
        private readonly AuthService mAuthService;

        // 当前用户
        private User mCurrentUser;

        // 编辑中的昵称
        private string mEditNickname;

        // 编辑中的手机号
        private string mEditPhone;

        // 旧密码
        private string mOldPassword;

        // 新密码
        private string mNewPassword;

        // 确认新密码
        private string mConfirmPassword;

        // 操作结果提示
        private string mResultMessage;

        // 结果是否为成功（控制颜色）
        private bool mIsResultSuccess;
        #region 属性绑定

        public User CurrentUser
        {
            get => mCurrentUser;
            set { mCurrentUser = value; OnPropertyChanged(); }
        }

        public string EditNickname
        {
            get => mEditNickname;
            set { mEditNickname = value; OnPropertyChanged(); }
        }

        public string EditPhone
        {
            get => mEditPhone;
            set { mEditPhone = value; OnPropertyChanged(); }
        }

        public string OldPassword
        {
            get => mOldPassword;
            set { mOldPassword = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get => mNewPassword;
            set { mNewPassword = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => mConfirmPassword;
            set { mConfirmPassword = value; OnPropertyChanged(); }
        }

        public string ResultMessage
        {
            get => mResultMessage;
            set { mResultMessage = value; OnPropertyChanged(); }
        }

        public bool IsResultSuccess
        {
            get => mIsResultSuccess;
            set { mIsResultSuccess = value; OnPropertyChanged(); }
        }

        /// <summary>角色显示文本</summary>
        public string RoleDisplayText =>
            mCurrentUser?.role == UserRole.Admin ? "👑 管理员" : "🌾 普通农户";

        #endregion

        #region 命令

        public ICommand SaveNicknameCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand BindPhoneCommand { get; }
        public ICommand ChangeAvatarCommand { get; }
        public ICommand LogoutCommand { get; }

        #endregion
        /// <summary>退出登录回调</summary>
        public System.Action OnLogoutCallback { get; set; }

        public ProfileViewModel(UserService userService, AuthService authService)
        {
            mUserService = userService;
            mAuthService = authService;
            mCurrentUser = authService.CurrentUser;

            // 初始化编辑字段
            mEditNickname = mCurrentUser?.nickname;
            mEditPhone = mCurrentUser?.phoneNumber;

            SaveNicknameCommand = new RelayCommand(
                async _ => await SaveNicknameAsync());
            ChangePasswordCommand = new RelayCommand(
                async _ => await ChangePasswordAsync());
            BindPhoneCommand = new RelayCommand(
                async _ => await BindPhoneAsync());
            ChangeAvatarCommand = new RelayCommand(
                async _ => await ChangeAvatarAsync());
            LogoutCommand = new RelayCommand(
                async _ => await LogoutAsync());
        }

        /// <summary>
        /// 保存昵称修改
        /// </summary>
        private async Task SaveNicknameAsync()
        {
            if (string.IsNullOrWhiteSpace(EditNickname))
            {
                ShowResult("昵称不能为空", false);
                return;
            }
            if (EditNickname.Length > 20)
            {
                ShowResult("昵称不能超过20个字符", false);
                return;
            }

            await mUserService.UpdateNicknameAsync(mCurrentUser.id, EditNickname);
            mCurrentUser.nickname = EditNickname;
            OnPropertyChanged(nameof(CurrentUser));
            ShowResult("昵称修改成功 ✅", true);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        private async Task ChangePasswordAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                ShowResult("两次输入的新密码不一致", false);
                return;
            }

            string error = await mUserService.ChangePasswordAsync(
                mCurrentUser.id, OldPassword, NewPassword);

            if (error == null)
            {
                ShowResult("密码修改成功，请重新登录 ✅", true);
                // 密码修改成功后退出登录
                await Task.Delay(1500);
                await LogoutAsync();
            }
            else
            {
                ShowResult(error, false);
            }
        }

        /// <summary>
        /// 绑定手机号（模拟发送验证码后直接绑定）
        /// </summary>
        private async Task BindPhoneAsync()
        {
            string error = await mUserService.BindPhoneAsync(
                mCurrentUser.id, EditPhone);

            if (error == null)
            {
                mCurrentUser.phoneNumber = EditPhone;
                ShowResult("手机号绑定成功 ✅", true);
            }
            else
            {
                ShowResult(error, false);
            }
        }
        /// <summary>
        /// 更换头像（打开文件选择对话框）
        /// </summary>
        private async Task ChangeAvatarAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择头像图片",
                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                await mUserService.UpdateAvatarAsync(mCurrentUser.id, filePath);
                mCurrentUser.avatarPath = filePath;
                OnPropertyChanged(nameof(CurrentUser));
                ShowResult("头像更新成功 ✅", true);
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        private async Task LogoutAsync()
        {
            await mAuthService.LogoutAsync();
            OnLogoutCallback?.Invoke();
        }

        /// <summary>
        /// 显示操作结果提示
        /// </summary>
        private void ShowResult(string message, bool isSuccess)
        {
            ResultMessage = message;
            IsResultSuccess = isSuccess;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}