using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Data.SqlClient;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class CropEditDialog : Window
    {
        private readonly DatabaseService mDb;
        private readonly CropInfo mCrop;
        private readonly bool mIsEdit;

        public CropEditDialog(CropInfo crop)
        {
            InitializeComponent();
            mDb = new DatabaseService();

            if (crop != null)
            {
                mIsEdit = true;
                mCrop = crop;
                Title = "编辑作物";
                TxtCropName.Text = crop.cropName ?? "";
                TxtVariety.Text = crop.variety ?? "";
                TxtTempMin.Text = crop.tempMin.ToString();
                TxtTempMax.Text = crop.tempMax.ToString();
                TxtHumidityMin.Text = crop.humidityMin.ToString();
                TxtHumidityMax.Text = crop.humidityMax.ToString();
                TxtGrowthCycle.Text = crop.growthCycleDays.ToString();
                TxtDescription.Text = crop.description ?? "";
            }
            else
            {
                mIsEdit = false;
                mCrop = new CropInfo();
                Title = "添加作物";
                TxtTempMin.Text = "0";
                TxtTempMax.Text = "50";
                TxtHumidityMin.Text = "0";
                TxtHumidityMax.Text = "100";
                TxtGrowthCycle.Text = "90";
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCropName.Text))
            {
                MessageBox.Show("请输入作物名称", "提示");
                return;
            }

            if (!double.TryParse(TxtTempMin.Text, out double tempMin) ||
                !double.TryParse(TxtTempMax.Text, out double tempMax) ||
                !double.TryParse(TxtHumidityMin.Text, out double humidityMin) ||
                !double.TryParse(TxtHumidityMax.Text, out double humidityMax) ||
                !int.TryParse(TxtGrowthCycle.Text, out int growthCycle))
            {
                MessageBox.Show("请输入有效的数值", "提示");
                return;
            }

            try
            {
                if (mIsEdit)
                {
                    await mDb.ExecuteNonQueryAsync(@"
UPDATE CropInfo SET cropName=@cropName, variety=@variety, tempMin=@tempMin, tempMax=@tempMax,
    humidityMin=@humidityMin, humidityMax=@humidityMax, growthCycleDays=@growthCycleDays, description=@description
WHERE id=@id",
                        new SqlParameter("@cropName", TxtCropName.Text.Trim()),
                        new SqlParameter("@variety", (object)TxtVariety.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@tempMin", tempMin),
                        new SqlParameter("@tempMax", tempMax),
                        new SqlParameter("@humidityMin", humidityMin),
                        new SqlParameter("@humidityMax", humidityMax),
                        new SqlParameter("@growthCycleDays", growthCycle),
                        new SqlParameter("@description", (object)TxtDescription.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@id", mCrop.id));
                }
                else
                {
                    await mDb.ExecuteNonQueryAsync(@"
INSERT INTO CropInfo (cropName, variety, tempMin, tempMax, humidityMin, humidityMax, growthCycleDays, description)
VALUES (@cropName, @variety, @tempMin, @tempMax, @humidityMin, @humidityMax, @growthCycleDays, @description)",
                        new SqlParameter("@cropName", TxtCropName.Text.Trim()),
                        new SqlParameter("@variety", (object)TxtVariety.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@tempMin", tempMin),
                        new SqlParameter("@tempMax", tempMax),
                        new SqlParameter("@humidityMin", humidityMin),
                        new SqlParameter("@humidityMax", humidityMax),
                        new SqlParameter("@growthCycleDays", growthCycle),
                        new SqlParameter("@description", (object)TxtDescription.Text.Trim() ?? DBNull.Value));
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
