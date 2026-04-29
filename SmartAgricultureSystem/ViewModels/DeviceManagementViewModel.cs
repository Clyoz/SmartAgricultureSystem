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
    public class DeviceManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService mDb;
        private Device mSelectedDevice;

        public ObservableCollection<Device> DeviceList { get; set; }

        public Device SelectedDevice
        {
            get => mSelectedDevice;
            set { mSelectedDevice = value; OnPropertyChanged(); }
        }

        public ICommand AddDeviceCommand { get; }
        public ICommand RefreshCommand { get; }

        public DeviceManagementViewModel()
        {
            mDb = new DatabaseService();
            DeviceList = new ObservableCollection<Device>();

            AddDeviceCommand = new RelayCommand(_ => AddDevice());
            RefreshCommand = new RelayCommand(async _ => await LoadDevicesAsync());

            LoadDevicesAsync();
        }

        private async System.Threading.Tasks.Task LoadDevicesAsync()
        {
            try
            {
                var devices = await mDb.GetSensorDevicesAsync();
                DeviceList.Clear();
                foreach (var device in devices)
                {
                    DeviceList.Add(device);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[设备管理] 加载失败: {ex.Message}");
            }
        }

        private void AddDevice()
        {
            var dialog = new Views.DeviceEditDialog(null);
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                LoadDevicesAsync();
            }
        }

        public async void DeleteDevice(int deviceId)
        {
            try
            {
                await mDb.ExecuteNonQueryAsync("DELETE FROM Devices WHERE id = @id",
                    new System.Data.SqlClient.SqlParameter("@id", deviceId));
                LoadDevicesAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"删除失败: {ex.Message}", "错误");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
