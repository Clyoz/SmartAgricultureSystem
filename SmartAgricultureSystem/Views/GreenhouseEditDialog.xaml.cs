using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Data.SqlClient;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class GreenhouseEditDialog : Window
    {
        private readonly DatabaseService mDb;
        private readonly Greenhouse mGreenhouse;
        private readonly bool mIsEdit;

        public GreenhouseEditDialog(Greenhouse greenhouse)
        {
            InitializeComponent();
            mDb = new DatabaseService();

            if (greenhouse != null)
            {
                mIsEdit = true;
                mGreenhouse = greenhouse;
                Title = "编辑大棚";
                TxtCode.Text = greenhouse.greenhouseCode ?? "";
                TxtName.Text = greenhouse.name ?? "";
                TxtLocation.Text = greenhouse.location ?? "";
                TxtArea.Text = greenhouse.area.ToString();
                TxtRemark.Text = greenhouse.remark ?? "";

                // 设置类型
                for (int i = 0; i < CmbType.Items.Count; i++)
                {
                    if ((CmbType.Items[i] as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() == greenhouse.greenhouseType)
                    {
                        CmbType.SelectedIndex = i;
                        break;
                    }
                }

                // 设置状态
                CmbStatus.SelectedIndex = greenhouse.status - 1;
                if (CmbStatus.SelectedIndex < 0) CmbStatus.SelectedIndex = 0;
            }
            else
            {
                mIsEdit = false;
                mGreenhouse = new Greenhouse();
                Title = "添加大棚";
                TxtArea.Text = "0";
                CmbType.SelectedIndex = 0;
                CmbStatus.SelectedIndex = 0;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCode.Text))
            {
                MessageBox.Show("请输入大棚编号", "提示");
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("请输入大棚名称", "提示");
                return;
            }
            if (!double.TryParse(TxtArea.Text, out double area))
            {
                MessageBox.Show("请输入有效的面积", "提示");
                return;
            }

            string ghType = (CmbType.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "塑料大棚";
            int status = CmbStatus.SelectedIndex + 1;

            try
            {
                if (mIsEdit)
                {
                    await mDb.ExecuteNonQueryAsync(@"
UPDATE Greenhouses SET greenhouseCode=@code, name=@name, location=@location, area=@area,
    greenhouseType=@type, status=@status, remark=@remark, updatedAt=GETDATE()
WHERE id=@id",
                        new SqlParameter("@code", TxtCode.Text.Trim()),
                        new SqlParameter("@name", TxtName.Text.Trim()),
                        new SqlParameter("@location", (object)TxtLocation.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@area", area),
                        new SqlParameter("@type", ghType),
                        new SqlParameter("@status", status),
                        new SqlParameter("@remark", (object)TxtRemark.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@id", mGreenhouse.id));
                }
                else
                {
                    await mDb.ExecuteNonQueryAsync(@"
INSERT INTO Greenhouses (greenhouseCode, name, location, area, greenhouseType, status, remark)
VALUES (@code, @name, @location, @area, @type, @status, @remark)",
                        new SqlParameter("@code", TxtCode.Text.Trim()),
                        new SqlParameter("@name", TxtName.Text.Trim()),
                        new SqlParameter("@location", (object)TxtLocation.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@area", area),
                        new SqlParameter("@type", ghType),
                        new SqlParameter("@status", status),
                        new SqlParameter("@remark", (object)TxtRemark.Text.Trim() ?? DBNull.Value));
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
