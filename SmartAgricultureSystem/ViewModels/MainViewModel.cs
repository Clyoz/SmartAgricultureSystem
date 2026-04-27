using LiveCharts;
using LiveCharts.Wpf;
using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartAgricultureSystem.ViewModels
{
    /// <summary>
    /// 主界面视图模型
    /// 实现 INotifyPropertyChanged 以支持数据绑定
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // Modbus通讯服务
        private ModbusService mModbusService;

        // 虚拟Modbus从站服务（无真实设备时使用）
        private VirtualModbusSlaveService mVirtualSlave;

        // 数据库服务
        private readonly DatabaseService mDatabaseService;

        // 认证服务（用于退出登录）
        private AuthService mAuthService;

        // 预警服务
        private readonly AlertService mAlertService;

        // 预警配置
        private readonly AlertConfig mAlertConfig;

        // 定时采集的取消令牌
        private CancellationTokenSource mCancellationTokenSource;

        // 单次读取的取消令牌（连接后定时读取当前值）
        private CancellationTokenSource mReadCurrentCts;

        // 当前温度
        private double mCurrentTemperature;

        // 当前湿度
        private double mCurrentHumidity;

        // 连接状态描述
        private string mConnectionStatus;

        // 最新预警信息
        private string mLatestAlert;

        // 是否正在运行采集
        private bool mIsRunning;

        // 是否使用虚拟模式
        private bool mIsVirtualMode;

        // 是否已连接
        private bool mIsConnected;

        // 设备IP地址
        private string mDeviceIp = "192.168.1.100";

        // 设备端口
        private int mDevicePort = 502;

        // 当前登录用户名
        private string mCurrentUsername;

        // 选中的设备
        private Device mSelectedDevice;

        // 图表时间标签集合
        public ObservableCollection<string> TimeLabels { get; set; }

        // 图表数据系列集合
        public SeriesCollection ChartSeries { get; set; }

        // 温度折线图数据
        private ChartValues<double> mTemperatureValues;

        // 湿度折线图数据
        private ChartValues<double> mHumidityValues;

        // 历史数据列表（用于DataGrid展示）
        public ObservableCollection<SensorData> HistoryData { get; set; }

        // 设备列表（从数据库加载）
        public ObservableCollection<Device> DeviceList { get; set; }

        #region 属性绑定

        /// <summary>当前温度（绑定到UI）</summary>
        public double CurrentTemperature
        {
            get => mCurrentTemperature;
            set { mCurrentTemperature = value; OnPropertyChanged(); }
        }

        /// <summary>当前湿度（绑定到UI）</summary>
        public double CurrentHumidity
        {
            get => mCurrentHumidity;
            set { mCurrentHumidity = value; OnPropertyChanged(); }
        }

        /// <summary>连接状态（绑定到UI）</summary>
        public string ConnectionStatus
        {
            get => mConnectionStatus;
            set { mConnectionStatus = value; OnPropertyChanged(); }
        }

        /// <summary>最新预警信息（绑定到UI）</summary>
        public string LatestAlert
        {
            get => mLatestAlert;
            set { mLatestAlert = value; OnPropertyChanged(); }
        }

        /// <summary>是否正在运行（绑定到UI）</summary>
        public bool IsRunning
        {
            get => mIsRunning;
            set { mIsRunning = value; OnPropertyChanged(); }
        }

        /// <summary>是否已连接（绑定到UI）</summary>
        public bool IsConnected
        {
            get => mIsConnected;
            set { mIsConnected = value; OnPropertyChanged(); }
        }

        /// <summary>设备IP地址（绑定到UI）</summary>
        public string DeviceIp
        {
            get => mDeviceIp;
            set { mDeviceIp = value; OnPropertyChanged(); }
        }

        /// <summary>设备端口（绑定到UI）</summary>
        public int DevicePort
        {
            get => mDevicePort;
            set { mDevicePort = value; OnPropertyChanged(); }
        }

        /// <summary>是否使用虚拟模式（绑定到UI）</summary>
        public bool IsVirtualMode
        {
            get => mIsVirtualMode;
            set { mIsVirtualMode = value; OnPropertyChanged(); }
        }

        /// <summary>当前登录用户名（绑定到UI）</summary>
        public string CurrentUsername
        {
            get => mCurrentUsername;
            set { mCurrentUsername = value; OnPropertyChanged(); }
        }

        /// <summary>选中的设备（绑定到UI的ComboBox）</summary>
        public Device SelectedDevice
        {
            get => mSelectedDevice;
            set
            {
                mSelectedDevice = value;
                OnPropertyChanged();
                // 选择设备后自动填充IP和端口
                if (mSelectedDevice != null && !IsVirtualMode)
                {
                    DeviceIp = mSelectedDevice.ipAddress ?? "";
                    DevicePort = mSelectedDevice.port;
                }
            }
        }

        #endregion
        #region 命令

        /// <summary>连接设备命令</summary>
        public ICommand ConnectCommand { get; }

        /// <summary>断开连接命令</summary>
        public ICommand DisconnectCommand { get; }

        /// <summary>开始采集命令</summary>
        public ICommand StartCollectCommand { get; }

        /// <summary>停止采集命令</summary>
        public ICommand StopCollectCommand { get; }

        /// <summary>切换虚拟模式命令</summary>
        public ICommand ToggleVirtualModeCommand { get; }

        /// <summary>退出登录命令</summary>
        public ICommand LogoutCommand { get; }

        /// <summary>打开个人中心命令</summary>
        public ICommand OpenProfileCommand { get; }

        #endregion

        /// <summary>退出登录回调</summary>
        public System.Action OnLogoutCallback { get; set; }

        /// <summary>打开个人中心回调</summary>
        public System.Action OnOpenProfileCallback { get; set; }

        /// <summary>
        /// 构造函数，初始化所有服务和命令
        /// </summary>
        public MainViewModel()
        {
            mAlertConfig = new AlertConfig();
            mDatabaseService = new DatabaseService();
            mAlertService = new AlertService(mAlertConfig);

            // 订阅预警事件
            mAlertService.OnAlert += OnAlertTriggered;

            // 初始化图表数据
            mTemperatureValues = new ChartValues<double>();
            mHumidityValues = new ChartValues<double>();
            TimeLabels = new ObservableCollection<string>();
            HistoryData = new ObservableCollection<SensorData>();
            DeviceList = new ObservableCollection<Device>();

            // 异步加载设备列表
            LoadDevicesAsync();

            // 初始化图表系列
            ChartSeries = new SeriesCollection {
                new LineSeries {
                    Title = "温度(℃)",
                    Values = mTemperatureValues,
                    PointGeometrySize = 5
                },
                new LineSeries {
                    Title = "湿度(%)",
                    Values = mHumidityValues,
                    PointGeometrySize = 5
                }
            };

            // 绑定命令
            ConnectCommand = new RelayCommand(async _ => await ConnectAsync(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(_ => Disconnect(), _ => IsConnected);
            StartCollectCommand = new RelayCommand(async _ => await StartCollectingAsync(), _ => IsConnected && !IsRunning);
            StopCollectCommand = new RelayCommand(_ => StopCollecting(), _ => IsRunning);
            ToggleVirtualModeCommand = new RelayCommand(async _ => await ToggleVirtualModeAsync());
            LogoutCommand = new RelayCommand(async _ => await LogoutAsync());
            OpenProfileCommand = new RelayCommand(_ => OpenProfile());

            ConnectionStatus = "未连接";
        }

        /// <summary>
        /// 设置当前用户信息
        /// </summary>
        public void SetCurrentUser(string username)
        {
            CurrentUsername = username;
        }

        /// <summary>
        /// 异步从数据库加载设备列表
        /// </summary>
        private async void LoadDevicesAsync()
        {
            try
            {
                var devices = await mDatabaseService.GetSensorDevicesAsync();
                DeviceList.Clear();
                foreach (var device in devices)
                {
                    DeviceList.Add(device);
                }
                // 默认选中第一个设备
                if (DeviceList.Count > 0)
                {
                    SelectedDevice = DeviceList[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[设备列表] 加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置认证服务（用于退出登录）
        /// </summary>
        public void SetAuthService(AuthService authService)
        {
            mAuthService = authService;
        }

        /// <summary>
        /// 打开个人中心
        /// </summary>
        private void OpenProfile()
        {
            OnOpenProfileCallback?.Invoke();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        private async Task LogoutAsync()
        {
            // 先停止采集和断开连接
            if (IsRunning) StopCollecting();
            if (IsConnected) Disconnect();

            // 调用AuthService清除服务端会话
            if (mAuthService != null)
            {
                await mAuthService.LogoutAsync();
            }

            OnLogoutCallback?.Invoke();
        }

        /// <summary>
        /// 异步连接到Modbus设备
        /// 连接后定时读取当前温湿度值（仅更新数值卡片，不更新曲线图）
        /// </summary>
        private async Task ConnectAsync()
        {
            // 输入验证
            if (IsVirtualMode)
            {
                // 虚拟模式无需验证IP
            }
            else
            {
                if (string.IsNullOrWhiteSpace(DeviceIp))
                {
                    MessageBox.Show("请输入IP地址！", "提示");
                    return;
                }
                if (DevicePort <= 0 || DevicePort > 65535)
                {
                    MessageBox.Show("请输入有效的端口号（1-65535）！", "提示");
                    return;
                }
            }

            ConnectionStatus = "连接中...";

            if (IsVirtualMode)
            {
                // 虚拟模式：启动本地从站，使用用户输入的端口（默认5020）
                int virtualPort = DevicePort > 0 ? DevicePort : 5020;
                try
                {
                    mVirtualSlave = new VirtualModbusSlaveService(virtualPort);
                    mVirtualSlave.Start();
                    // 短暂等待从站启动
                    await Task.Delay(500);
                    mModbusService = new ModbusService("127.0.0.1", virtualPort);
                    bool success = await mModbusService.ConnectAsync();
                    if (success)
                    {
                        IsConnected = true;
                        ConnectionStatus = $"已连接 (虚拟模式 127.0.0.1:{virtualPort})";
                        StartReadingCurrentValues();
                    }
                    else
                    {
                        mVirtualSlave.Stop();
                        ConnectionStatus = "虚拟模式连接失败";
                    }
                }
                catch (Exception ex)
                {
                    mVirtualSlave?.Stop();
                    ConnectionStatus = $"虚拟模式启动失败: {ex.Message}";
                }
            }
            else
            {
                // 真实设备模式
                try
                {
                    // 使用选中设备的slaveId，如果未选中则默认1
                    byte slaveId = SelectedDevice?.slaveId ?? (byte)1;
                    mModbusService = new ModbusService(DeviceIp, DevicePort, slaveId);
                    bool success = await mModbusService.ConnectAsync();
                    if (success)
                    {
                        IsConnected = true;
                        ConnectionStatus = $"已连接 ({DeviceIp}:{DevicePort})";
                        StartReadingCurrentValues();

                        // 更新设备在线状态
                        if (SelectedDevice != null)
                        {
                            try { await mDatabaseService.UpdateDeviceOnlineStatusAsync(SelectedDevice.id, true); }
                            catch { /* 忽略数据库更新失败 */ }
                        }
                    }
                    else
                    {
                        ConnectionStatus = "连接失败，请检查IP地址和端口号";
                    }
                }
                catch (Exception ex)
                {
                    ConnectionStatus = $"连接失败: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 连接后启动定时读取当前温湿度值（仅更新数值卡片，不更新曲线图）
        /// </summary>
        private void StartReadingCurrentValues()
        {
            mReadCurrentCts = new CancellationTokenSource();
            var token = mReadCurrentCts.Token;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && mModbusService != null && mModbusService.IsConnected)
                {
                    try
                    {
                        var (temp, humidity) = await mModbusService.ReadTempHumidityAsync();

                        if (!double.IsNaN(temp) && !double.IsNaN(humidity))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                CurrentTemperature = temp;
                                CurrentHumidity = humidity;
                            });
                        }

                        await Task.Delay(1000, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[读取当前值] 异常: {ex.Message}");
                    }
                }
            }, token);
        }

        /// <summary>
        /// 停止定时读取当前值
        /// </summary>
        private void StopReadingCurrentValues()
        {
            mReadCurrentCts?.Cancel();
        }

        /// <summary>
        /// 断开设备连接
        /// IP地址和端口号输入栏清空，温湿度曲线图初始化
        /// </summary>
        private void Disconnect()
        {
            // 停止采集（如果正在运行）
            StopCollecting();
            // 停止读取当前值
            StopReadingCurrentValues();

            mModbusService?.Dispose();
            mVirtualSlave?.Stop();

            // 更新设备离线状态
            if (SelectedDevice != null && !IsVirtualMode)
            {
                try { mDatabaseService.UpdateDeviceOnlineStatusAsync(SelectedDevice.id, false).Wait(); }
                catch { /* 忽略 */ }
            }

            IsConnected = false;
            ConnectionStatus = "未连接";

            // 清空IP地址和端口号（虚拟模式保留默认端口）
            DeviceIp = string.Empty;
            DevicePort = IsVirtualMode ? 5020 : 0;

            // 重置当前温湿度显示
            CurrentTemperature = 0;
            CurrentHumidity = 0;

            // 初始化温湿度曲线图（清空所有数据点）
            mTemperatureValues.Clear();
            mHumidityValues.Clear();
            TimeLabels.Clear();

            // 清空历史数据列表
            HistoryData.Clear();

            // 清空预警信息
            LatestAlert = string.Empty;
        }

        /// <summary>
        /// 开始定时采集数据
        /// 根据传感器的温湿度数据，更新温湿度监测曲线图，并能根据时间实时更新
        /// </summary>
        private async Task StartCollectingAsync()
        {
            if (mModbusService == null || !mModbusService.IsConnected)
            {
                MessageBox.Show("请先连接设备！", "提示");
                return;
            }

            // 停止简单的当前值读取，采集模式会包含更完整的数据处理
            StopReadingCurrentValues();

            IsRunning = true;
            mCancellationTokenSource = new CancellationTokenSource();
            var token = mCancellationTokenSource.Token;

            // 在后台线程循环采集
            await Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // 读取温湿度数据
                        var (temp, humidity) = await mModbusService.ReadTempHumidityAsync();

                        if (!double.IsNaN(temp) && !double.IsNaN(humidity))
                        {
                            var data = new SensorData
                            {
                                sensorId = 1,
                                deviceId = 1,
                                greenhouseId = 1,
                                temperature = temp,
                                humidity = humidity,
                                timestamp = DateTime.Now,
                                isAlert = false
                            };

                            // 检查预警
                            mAlertService.CheckAndAlert(data);

                            // 保存到数据库
                            await mDatabaseService.SaveDataAsync(data);

                            // 更新UI（必须在UI线程执行）
                            Application.Current.Dispatcher.Invoke(() => {
                                CurrentTemperature = temp;
                                CurrentHumidity = humidity;

                                // 更新温湿度监测曲线图
                                if (mTemperatureValues.Count >= 50)
                                {
                                    mTemperatureValues.RemoveAt(0);
                                    mHumidityValues.RemoveAt(0);
                                    TimeLabels.RemoveAt(0);
                                }

                                mTemperatureValues.Add(temp);
                                mHumidityValues.Add(humidity);
                                TimeLabels.Add(DateTime.Now.ToString("HH:mm:ss"));

                                // 更新历史列表（最多显示20条）
                                HistoryData.Insert(0, data);
                                if (HistoryData.Count > 20)
                                {
                                    HistoryData.RemoveAt(HistoryData.Count - 1);
                                }
                            });
                        }

                        // 按配置间隔等待
                        await Task.Delay(mAlertConfig.POLL_INTERVAL_MS, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[采集] 异常: {ex.Message}");
                    }
                }
            }, token);
        }

        /// <summary>
        /// 切换虚拟模式
        /// </summary>
        private async Task ToggleVirtualModeAsync()
        {
            if (IsRunning)
            {
                MessageBox.Show("请先停止采集再切换模式！", "提示");
                IsVirtualMode = !IsVirtualMode; // 还原切换
                return;
            }

            if (mModbusService != null && mModbusService.IsConnected)
            {
                Disconnect();
            }

            if (IsVirtualMode)
            {
                ConnectionStatus = "虚拟模式（未连接）";
                DeviceIp = "127.0.0.1";
                DevicePort = 5020;
            }
            else
            {
                ConnectionStatus = "未连接";
                // 恢复选中设备的IP和端口
                if (SelectedDevice != null)
                {
                    DeviceIp = SelectedDevice.ipAddress ?? "192.168.1.100";
                    DevicePort = SelectedDevice.port;
                }
                else if (DeviceList.Count > 0)
                {
                    SelectedDevice = DeviceList[0];
                }
                else
                {
                    DeviceIp = "192.168.1.100";
                    DevicePort = 502;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 停止数据采集
        /// 曲线图取消实时更新，但保留已有数据
        /// </summary>
        private void StopCollecting()
        {
            mCancellationTokenSource?.Cancel();
            IsRunning = false;

            // 停止采集后，恢复简单的当前值读取
            if (IsConnected && mModbusService != null && mModbusService.IsConnected)
            {
                StartReadingCurrentValues();
            }
        }

        /// <summary>
        /// 预警触发回调
        /// </summary>
        /// <param name="message">预警消息</param>
        private void OnAlertTriggered(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                LatestAlert = $"[{DateTime.Now:HH:mm:ss}] {message}";
            });
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}