using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SmartAgricultureSystem.ViewModels
{
    public class CropManagementViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService mDb;
        private CropInfo mSelectedCrop;
        private string mSearchKeyword;

        public ObservableCollection<CropInfo> CropList { get; set; }

        public CropInfo SelectedCrop
        {
            get => mSelectedCrop;
            set { mSelectedCrop = value; OnPropertyChanged(); }
        }

        public string SearchKeyword
        {
            get => mSearchKeyword;
            set { mSearchKeyword = value; OnPropertyChanged(); FilterCrops(); }
        }

        public ICommand AddCropCommand { get; }
        public ICommand RefreshCommand { get; }

        private ObservableCollection<CropInfo> mAllCrops;

        public CropManagementViewModel()
        {
            mDb = new DatabaseService();
            CropList = new ObservableCollection<CropInfo>();
            mAllCrops = new ObservableCollection<CropInfo>();

            AddCropCommand = new RelayCommand(_ => AddCrop());
            RefreshCommand = new RelayCommand(async _ => await LoadCropsAsync());

            LoadCropsAsync();
        }

        private async System.Threading.Tasks.Task LoadCropsAsync()
        {
            try
            {
                var dt = await mDb.ExecuteQueryAsync("SELECT * FROM CropInfo ORDER BY id");
                mAllCrops.Clear();
                CropList.Clear();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    var crop = new CropInfo
                    {
                        id = Convert.ToInt32(row["id"]),
                        cropName = row["cropName"]?.ToString(),
                        variety = row["variety"]?.ToString(),
                        tempMin = Convert.ToDouble(row["tempMin"]),
                        tempMax = Convert.ToDouble(row["tempMax"]),
                        humidityMin = Convert.ToDouble(row["humidityMin"]),
                        humidityMax = Convert.ToDouble(row["humidityMax"]),
                        lightMin = row["lightMin"] == System.DBNull.Value ? (double?)null : Convert.ToDouble(row["lightMin"]),
                        lightMax = row["lightMax"] == System.DBNull.Value ? (double?)null : Convert.ToDouble(row["lightMax"]),
                        growthCycleDays = Convert.ToInt32(row["growthCycleDays"]),
                        description = row["description"]?.ToString(),
                        createdAt = Convert.ToDateTime(row["createdAt"])
                    };
                    mAllCrops.Add(crop);
                    CropList.Add(crop);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[蔬菜管理] 加载失败: {ex.Message}");
            }
        }

        private void FilterCrops()
        {
            CropList.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchKeyword)
                ? mAllCrops
                : mAllCrops.Where(c => (c.cropName?.Contains(SearchKeyword) == true) ||
                                       (c.variety?.Contains(SearchKeyword) == true));
            foreach (var crop in filtered)
                CropList.Add(crop);
        }

        private void AddCrop()
        {
            var dialog = new Views.CropEditDialog(null);
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                LoadCropsAsync();
            }
        }

        public async void DeleteCrop(int cropId)
        {
            try
            {
                await mDb.ExecuteNonQueryAsync("DELETE FROM CropInfo WHERE id = @id",
                    new System.Data.SqlClient.SqlParameter("@id", cropId));
                LoadCropsAsync();
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
