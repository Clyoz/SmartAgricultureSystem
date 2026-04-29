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
    public class GreenhouseManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService mDb;
        private Greenhouse mSelectedGreenhouse;

        public ObservableCollection<Greenhouse> GreenhouseList { get; set; }

        public Greenhouse SelectedGreenhouse
        {
            get => mSelectedGreenhouse;
            set { mSelectedGreenhouse = value; OnPropertyChanged(); }
        }

        public ICommand AddGreenhouseCommand { get; }
        public ICommand RefreshCommand { get; }

        public GreenhouseManagementViewModel()
        {
            mDb = new DatabaseService();
            GreenhouseList = new ObservableCollection<Greenhouse>();

            AddGreenhouseCommand = new RelayCommand(_ => AddGreenhouse());
            RefreshCommand = new RelayCommand(async _ => await LoadGreenhousesAsync());

            LoadGreenhousesAsync();
        }

        private async System.Threading.Tasks.Task LoadGreenhousesAsync()
        {
            try
            {
                var dt = await mDb.ExecuteQueryAsync("SELECT * FROM Greenhouses ORDER BY id");
                GreenhouseList.Clear();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    GreenhouseList.Add(new Greenhouse
                    {
                        id = Convert.ToInt32(row["id"]),
                        greenhouseCode = row["greenhouseCode"]?.ToString(),
                        name = row["name"]?.ToString(),
                        location = row["location"]?.ToString(),
                        area = row["area"] == System.DBNull.Value ? 0 : Convert.ToDouble(row["area"]),
                        greenhouseType = row["greenhouseType"]?.ToString(),
                        managerId = row["managerId"] == System.DBNull.Value ? (int?)null : Convert.ToInt32(row["managerId"]),
                        buildDate = row["buildDate"] == System.DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["buildDate"]),
                        status = Convert.ToInt32(row["status"]),
                        remark = row["remark"]?.ToString(),
                        createdAt = Convert.ToDateTime(row["createdAt"]),
                        updatedAt = row["updatedAt"] == System.DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["updatedAt"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[大棚管理] 加载失败: {ex.Message}");
            }
        }

        private void AddGreenhouse()
        {
            var dialog = new Views.GreenhouseEditDialog(null);
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                LoadGreenhousesAsync();
            }
        }

        public async void DeleteGreenhouse(int greenhouseId)
        {
            try
            {
                await mDb.ExecuteNonQueryAsync("DELETE FROM Greenhouses WHERE id = @id",
                    new System.Data.SqlClient.SqlParameter("@id", greenhouseId));
                LoadGreenhousesAsync();
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
