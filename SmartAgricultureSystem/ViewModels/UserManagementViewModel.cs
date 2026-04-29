using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SmartAgricultureSystem.ViewModels
{
    public class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly UserService mUserService;
        private User mSelectedUser;

        public ObservableCollection<User> UserList { get; set; }

        public User SelectedUser
        {
            get => mSelectedUser;
            set { mSelectedUser = value; OnPropertyChanged(); }
        }

        public ICommand AddUserCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ShowLoginLogsCommand { get; }

        public UserManagementViewModel()
        {
            mUserService = new UserService();
            UserList = new ObservableCollection<User>();

            AddUserCommand = new RelayCommand(_ => AddUser());
            RefreshCommand = new RelayCommand(async _ => await LoadUsersAsync());
            ShowLoginLogsCommand = new RelayCommand(_ => ShowLoginLogs());

            LoadUsersAsync();
        }

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            try
            {
                var users = await mUserService.GetAllUsersAsync();
                UserList.Clear();
                foreach (var user in users)
                {
                    UserList.Add(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[人员管理] 加载失败: {ex.Message}");
            }
        }

        private void AddUser()
        {
            var dialog = new Views.UserEditDialog(null);
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                LoadUsersAsync();
            }
        }

        public async void DeleteUser(int userId)
        {
            try
            {
                await mUserService.DeleteUserAsync(userId);
                LoadUsersAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"删除失败: {ex.Message}", "错误");
            }
        }

        public async void UnlockUser(int userId)
        {
            try
            {
                await mUserService.UnlockUserAsync(userId);
                System.Windows.MessageBox.Show("用户已解锁", "提示");
                LoadUsersAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"解锁失败: {ex.Message}", "错误");
            }
        }

        private void ShowLoginLogs()
        {
            var dialog = new Views.LoginLogsWindow();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
