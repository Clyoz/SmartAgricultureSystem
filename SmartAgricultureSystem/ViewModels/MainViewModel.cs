using LiveCharts;
using LiveCharts.Wpf;
using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Collections.Generic;
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

        // 设备所属大棚位置
        private string mDeviceLocation;

        // 当前用户角色
        private UserRole mCurrentUserRole;

        // 选中的导航项
        private int mSelectedNavIndex = -1;

        // 选中的设备
        private Device mSelectedDevice;

        // 人员管理ViewModel（仅管理员使用）
        private UserManagementViewModel mUserManagementVM;

        // 蔬菜管理ViewModel
        private CropManagementViewModel mCropManagementVM;

        // 大棚管理ViewModel
        private GreenhouseManagementViewModel mGreenhouseManagementVM;

        // 设备管理ViewModel
        private DeviceManagementViewModel mDeviceManagementVM;

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

        // 大棚监控数据列表
        public ObservableCollection<GreenhouseMonitorData> GreenhouseMonitorList { get; set; }

        // 预警状态记录（用于检测状态变化，避免重复弹窗）
        private readonly Dictionary<int, int> mPreviousTempStatus = new Dictionary<int, int>();
        private readonly Dictionary<int, int> mPreviousHumStatus = new Dictionary<int, int>();

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

        /// <summary>设备所属大棚位置（连接后显示）</summary>
        public string DeviceLocation
        {
            get => mDeviceLocation;
            set { mDeviceLocation = value; OnPropertyChanged(); }
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

        /// <summary>当前用户角色（绑定到UI）</summary>
        public UserRole CurrentUserRole
        {
            get => mCurrentUserRole;
            set { mCurrentUserRole = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsAdmin)); }
        }

        /// <summary>是否为管理员（绑定到UI）</summary>
        public bool IsAdmin => CurrentUserRole == UserRole.Admin;

        /// <summary>选中的导航项索引</summary>
        public int SelectedNavIndex
        {
            get => mSelectedNavIndex;
            set { mSelectedNavIndex = value; OnPropertyChanged(); }
        }

        /// <summary>人员管理ViewModel（仅管理员）</summary>
        public UserManagementViewModel UserManagementVM
        {
            get => mUserManagementVM;
            set { mUserManagementVM = value; OnPropertyChanged(); }
        }

        /// <summary>蔬菜管理ViewModel</summary>
        public CropManagementViewModel CropManagementVM
        {
            get => mCropManagementVM;
            set { mCropManagementVM = value; OnPropertyChanged(); }
        }

        /// <summary>大棚管理ViewModel</summary>
        public GreenhouseManagementViewModel GreenhouseManagementVM
        {
            get => mGreenhouseManagementVM;
            set { mGreenhouseManagementVM = value; OnPropertyChanged(); }
        }

        /// <summary>设备管理ViewModel</summary>
        public DeviceManagementViewModel DeviceManagementVM
        {
            get => mDeviceManagementVM;
            set { mDeviceManagementVM = value; OnPropertyChanged(); }
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
                if (mSelectedDevice != null)
                {
                    if (IsVirtualMode)
                    {
                        // 虚拟模式：自动填充127.0.0.1和默认端口
                        DeviceIp = "127.0.0.1";
                        DevicePort = 5020;
                    }
                    else
                    {
                        DeviceIp = mSelectedDevice.ipAddress ?? "";
                        DevicePort = mSelectedDevice.port;
                    }
                    // 显示设备所属大棚
                    DeviceLocation = !string.IsNullOrEmpty(mSelectedDevice.greenhouseName)
                        ? $"📍 {mSelectedDevice.greenhouseName}"
                        : "";
                }
                else
                {
                    DeviceIp = "";
                    DevicePort = 0;
                    DeviceLocation = "";
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

        /// <summary>导航命令</summary>
        public ICommand NavigateCommand { get; }

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
            GreenhouseMonitorList = new ObservableCollection<GreenhouseMonitorData>();

            // 异步加载大棚监控数据
            LoadGreenhouseMonitorDataAsync();

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
            NavigateCommand = new RelayCommand(param =>
            {
                if (param is int idx)
                    SelectedNavIndex = idx;
                else if (param is string s && int.TryParse(s, out int parsed))
                    SelectedNavIndex = parsed;
            });

            IsVirtualMode = true;
            ConnectionStatus = "未连接";
        }

        /// <summary>
        /// 设置当前用户信息
        /// </summary>
        public void SetCurrentUser(string username, UserRole role = UserRole.Farmer)
        {
            CurrentUsername = username;
            CurrentUserRole = role;

            // 初始化所有管理模块的ViewModel
            CropManagementVM = new CropManagementViewModel();
            GreenhouseManagementVM = new GreenhouseManagementViewModel();
            DeviceManagementVM = new DeviceManagementViewModel();

            if (IsAdmin)
            {
                UserManagementVM = new UserManagementViewModel();
            }
            // 所有用户默认显示数据监控
            SelectedNavIndex = 0;

            // 初始为未连接状态，数据为0
            ConnectionStatus = "未连接 · 点击连接开始实时监控";
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
        /// 异步加载大棚监控数据（初始温湿度为0，连接后才开始实时更新）
        /// </summary>
        private async void LoadGreenhouseMonitorDataAsync()
        {
            try
            {
                var data = await mDatabaseService.GetGreenhouseMonitorDataAsync();
                GreenhouseMonitorList.Clear();
                mPreviousTempStatus.Clear();
                mPreviousHumStatus.Clear();
                foreach (var item in data)
                {
                    item.CurrentTemperature = 0;
                    item.CurrentHumidity = 0;
                    item.LastUpdateTime = null;
                    item.IsStabilized = false;
                    // 初始目标值设为阈值中间值
                    item.TargetTemperature = Math.Round((item.tempMin + item.tempMax) / 2, 1);
                    item.TargetHumidity = Math.Round((item.humidityMin + item.humidityMax) / 2, 1);
                    GreenhouseMonitorList.Add(item);
                    mPreviousTempStatus[item.greenhouseId] = 0;
                    mPreviousHumStatus[item.greenhouseId] = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[大棚监控] 加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 虚拟模式下模拟所有大棚的温湿度数据变化
        /// </summary>
        private CancellationTokenSource mVirtualMonitorCts;

        private void StartVirtualMonitorSimulation()
        {
            StopVirtualMonitorSimulation();
            mVirtualMonitorCts = new CancellationTokenSource();
            var token = mVirtualMonitorCts.Token;

            var random = new Random();
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var gh in GreenhouseMonitorList)
                            {
                                double targetTemp = gh.TargetTemperature;
                                double targetHum = gh.TargetHumidity;

                                // 添加小幅随机波动模拟真实传感器
                                double tempNoise = (random.NextDouble() - 0.5) * 1.0;
                                double humNoise = (random.NextDouble() - 0.5) * 1.5;

                                // 直接设置为目标值 + 噪声，无平滑过渡
                                double newTemp = Math.Round(targetTemp + tempNoise, 1);
                                double newHum = Math.Round(targetHum + humNoise, 1);

                                gh.CurrentTemperature = newTemp;
                                gh.CurrentHumidity = newHum;
                                gh.LastUpdateTime = DateTime.Now;

                                // 首次赋值后直接标记为稳定，初始化预警状态
                                if (!gh.IsStabilized)
                                {
                                    gh.IsStabilized = true;
                                    mPreviousTempStatus[gh.greenhouseId] = gh.TempStatus;
                                    mPreviousHumStatus[gh.greenhouseId] = gh.HumidityStatus;
                                }

                                // 检查预警并弹窗
                                CheckAndAlertGreenhouse(gh);
                            }
                        });
                        await Task.Delay(2000, token);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[虚拟监控] 异常: {ex.Message}");
                    }
                }
            }, token);
        }

        private void StopVirtualMonitorSimulation()
        {
            mVirtualMonitorCts?.Cancel();
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
        /// 连接设备，启动大棚数据实时更新
        /// </summary>
        private async Task ConnectAsync()
        {
            if (IsConnected) return;

            ConnectionStatus = "连接中...";

            try
            {
                // 启动本地虚拟从站
                int virtualPort = 5020;
                mVirtualSlave = new VirtualModbusSlaveService(virtualPort);
                mVirtualSlave.Start();
                await Task.Delay(500);

                // 连接到虚拟从站
                mModbusService = new ModbusService("127.0.0.1", virtualPort);
                bool success = await mModbusService.ConnectAsync();
                if (success)
                {
                    IsConnected = true;
                    ConnectionStatus = "已连接 · 数据实时更新中";
                    StartReadingCurrentValues();
                    StartVirtualMonitorSimulation();
                }
                else
                {
                    mVirtualSlave.Stop();
                    ConnectionStatus = "连接失败";
                }
            }
            catch (Exception ex)
            {
                mVirtualSlave?.Stop();
                ConnectionStatus = $"连接失败: {ex.Message}";
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
        /// 断开设备连接，停止数据更新，重置所有大棚数据为0
        /// </summary>
        private async void Disconnect()
        {
            // 停止采集
            StopCollecting();
            // 停止读取当前值
            StopReadingCurrentValues();
            // 停止大棚数据模拟
            StopVirtualMonitorSimulation();

            mModbusService?.Dispose();
            mVirtualSlave?.Stop();

            IsConnected = false;
            IsVirtualMode = true;
            ConnectionStatus = "未连接";

            // 重置当前温湿度显示
            CurrentTemperature = 0;
            CurrentHumidity = 0;

            // 重置所有大棚监控数据为0
            foreach (var gh in GreenhouseMonitorList)
            {
                gh.CurrentTemperature = 0;
                gh.CurrentHumidity = 0;
                gh.LastUpdateTime = null;
                gh.IsStabilized = false;
            }
            mPreviousTempStatus.Clear();
            mPreviousHumStatus.Clear();

            // 清空历史数据
            HistoryData.Clear();
            mTemperatureValues.Clear();
            mHumidityValues.Clear();
            TimeLabels.Clear();

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
        /// <summary>
        /// 切换虚拟模式（已弃用，保留接口兼容性）
        /// </summary>
        private async Task ToggleVirtualModeAsync()
        {
            IsVirtualMode = true;
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
        /// 检查单个大棚的预警状态并弹窗提示
        /// </summary>
        private void CheckAndAlertGreenhouse(GreenhouseMonitorData gh)
        {
            int prevTempStatus = mPreviousTempStatus.ContainsKey(gh.greenhouseId) ? mPreviousTempStatus[gh.greenhouseId] : 0;
            int prevHumStatus = mPreviousHumStatus.ContainsKey(gh.greenhouseId) ? mPreviousHumStatus[gh.greenhouseId] : 0;

            int curTempStatus = gh.TempStatus;
            int curHumStatus = gh.HumidityStatus;

            // 温度预警弹窗：状态变化时触发
            if (curTempStatus != prevTempStatus)
            {
                if (curTempStatus == 2)
                {
                    string direction = gh.CurrentTemperature <= gh.tempMin ? "低于最低阈值" : "超过最高阈值";
                    ShowAlertPopup($"【警报】{gh.greenhouseName} 温度{direction}！\n当前温度: {gh.CurrentTemperature:F1}℃ (阈值: {gh.tempMin}~{gh.tempMax}℃)", true);
                    LatestAlert = $"[{DateTime.Now:HH:mm:ss}] {gh.greenhouseName} 温度{direction}: {gh.CurrentTemperature:F1}℃";
                }
                else if (curTempStatus == 1)
                {
                    ShowAlertPopup($"【预警】{gh.greenhouseName} 温度接近阈值！\n当前温度: {gh.CurrentTemperature:F1}℃ (阈值: {gh.tempMin}~{gh.tempMax}℃)", false);
                    LatestAlert = $"[{DateTime.Now:HH:mm:ss}] {gh.greenhouseName} 温度接近阈值: {gh.CurrentTemperature:F1}℃";
                }
            }

            // 湿度预警弹窗：状态变化时触发
            if (curHumStatus != prevHumStatus)
            {
                if (curHumStatus == 2)
                {
                    string direction = gh.CurrentHumidity <= gh.humidityMin ? "低于最低阈值" : "超过最高阈值";
                    ShowAlertPopup($"【警报】{gh.greenhouseName} 湿度{direction}！\n当前湿度: {gh.CurrentHumidity:F1}% (阈值: {gh.humidityMin}~{gh.humidityMax}%)", true);
                    LatestAlert = $"[{DateTime.Now:HH:mm:ss}] {gh.greenhouseName} 湿度{direction}: {gh.CurrentHumidity:F1}%";
                }
                else if (curHumStatus == 1)
                {
                    ShowAlertPopup($"【预警】{gh.greenhouseName} 湿度接近阈值！\n当前湿度: {gh.CurrentHumidity:F1}% (阈值: {gh.humidityMin}~{gh.humidityMax}%)", false);
                    LatestAlert = $"[{DateTime.Now:HH:mm:ss}] {gh.greenhouseName} 湿度接近阈值: {gh.CurrentHumidity:F1}%";
                }
            }

            mPreviousTempStatus[gh.greenhouseId] = curTempStatus;
            mPreviousHumStatus[gh.greenhouseId] = curHumStatus;
        }

        /// <summary>
        /// 显示预警弹窗（异步避免阻塞模拟线程）
        /// </summary>
        private void ShowAlertPopup(string message, bool isAlarm)
        {
            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isAlarm)
                    System.Windows.MessageBox.Show(message, "温湿度警报", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    System.Windows.MessageBox.Show(message, "温湿度预警", MessageBoxButton.OK, MessageBoxImage.Warning);
            }));
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
