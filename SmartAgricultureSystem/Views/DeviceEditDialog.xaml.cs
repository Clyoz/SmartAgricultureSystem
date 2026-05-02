using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class DeviceEditDialog : Window
    {
        private readonly DatabaseService mDb;
        private readonly Device mDevice;
        private readonly bool mIsEdit;

        public DeviceEditDialog(Device device)
        {
            InitializeComponent();
            mDb = new DatabaseService();

            // 加载大棚列表到下拉框
            LoadGreenhouses();

            if (device != null)
            {
                mIsEdit = true;
                mDevice = device;
                Title = "编辑设备";
                TxtDeviceCode.Text = device.deviceCode ?? "";
                TxtName.Text = device.name ?? "";
                // 设置选中大棚
                CmbGreenhouse.SelectedValue = device.greenhouseId;
                CmbDeviceType.SelectedIndex = device.deviceType - 1;
                if (CmbDeviceType.SelectedIndex < 0) CmbDeviceType.SelectedIndex = 1;
                TxtIpAddress.Text = device.ipAddress ?? "";
                TxtPort.Text = device.port.ToString();
                TxtSlaveId.Text = device.slaveId.ToString();
                TxtModel.Text = device.model ?? "";
                TxtFirmware.Text = device.firmwareVersion ?? "";
                TxtRemark.Text = device.remark ?? "";
            }
            else
            {
                mIsEdit = false;
                mDevice = new Device();
                Title = "添加设备";
                CmbGreenhouse.SelectedIndex = 0;
                CmbDeviceType.SelectedIndex = 1;
                TxtIpAddress.Text = "192.168.1.100";
                TxtPort.Text = "502";
                TxtSlaveId.Text = "1";
            }
        }

        private async void LoadGreenhouses()
        {
            try
            {
                var dt = await mDb.ExecuteQueryAsync("SELECT id, name FROM Greenhouses ORDER BY id");
                var greenhouses = new ObservableCollection<Greenhouse>();
                foreach (DataRow row in dt.Rows)
                {
                    greenhouses.Add(new Greenhouse
                    {
                        id = Convert.ToInt32(row["id"]),
                        name = row["name"]?.ToString()
                    });
                }
                CmbGreenhouse.ItemsSource = greenhouses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[设备编辑] 加载大棚列表失败: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDeviceCode.Text))
            {
                MessageBox.Show("请输入设备编号", "提示");
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("请输入设备名称", "提示");
                return;
            }
            int? ghId = CmbGreenhouse.SelectedValue as int?;
            if (ghId == null)
            {
                MessageBox.Show("请选择所属大棚", "提示");
                return;
            }
            if (!int.TryParse(TxtPort.Text, out int port) ||
                !byte.TryParse(TxtSlaveId.Text, out byte slaveId))
            {
                MessageBox.Show("请输入有效的数值", "提示");
                return;
            }

            int deviceType = CmbDeviceType.SelectedIndex + 1;
            if (deviceType < 1) deviceType = 2;

            try
            {
                if (mIsEdit)
                {
                    await mDb.ExecuteNonQueryAsync(@"
UPDATE Devices SET deviceCode=@code, name=@name, greenhouseId=@ghId, deviceType=@type,
    ipAddress=@ip, port=@port, slaveId=@slaveId, model=@model, firmwareVersion=@fw, remark=@remark, updatedAt=GETDATE()
WHERE id=@id",
                        new SqlParameter("@code", TxtDeviceCode.Text.Trim()),
                        new SqlParameter("@name", TxtName.Text.Trim()),
                        new SqlParameter("@ghId", ghId),
                        new SqlParameter("@type", deviceType),
                        new SqlParameter("@ip", (object)TxtIpAddress.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@port", port),
                        new SqlParameter("@slaveId", slaveId),
                        new SqlParameter("@model", (object)TxtModel.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@fw", (object)TxtFirmware.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@remark", (object)TxtRemark.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@id", mDevice.id));
                }
                else
                {
                    await mDb.ExecuteNonQueryAsync(@"
INSERT INTO Devices (deviceCode, name, greenhouseId, deviceType, ipAddress, port, slaveId, model, firmwareVersion, remark)
VALUES (@code, @name, @ghId, @type, @ip, @port, @slaveId, @model, @fw, @remark)",
                        new SqlParameter("@code", TxtDeviceCode.Text.Trim()),
                        new SqlParameter("@name", TxtName.Text.Trim()),
                        new SqlParameter("@ghId", ghId),
                        new SqlParameter("@type", deviceType),
                        new SqlParameter("@ip", (object)TxtIpAddress.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@port", port),
                        new SqlParameter("@slaveId", slaveId),
                        new SqlParameter("@model", (object)TxtModel.Text.Trim() ?? DBNull.Value),
                        new SqlParameter("@fw", (object)TxtFirmware.Text.Trim() ?? DBNull.Value),
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
