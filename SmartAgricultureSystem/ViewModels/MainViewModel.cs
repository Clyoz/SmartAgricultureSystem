using LiveCharts;
using LiveCharts.Wpf;
using SmartAgricultureSystem.Helpers;
using SmartAgricultureSystem.Models;
using SmartAgricultureSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        // 预警服务
        private readonly AlertService mAlertService;

        // 预警配置
        private readonly AlertConfig mAlertConfig;

        // 定时采集的取消令牌
        private CancellationTokenSource mCancellationTokenSource;

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

        // 设备IP地址
        private string mDeviceIp = "192.168.1.100";

        // 设备端口
        private int mDevicePort = 502;

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

        #endregion

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
            ConnectCommand = new RelayCommand(async _ => await ConnectAsync());
            DisconnectCommand = new RelayCommand(_ => Disconnect());
            StartCollectCommand = new RelayCommand(async _ => await StartCollectingAsync());
            StopCollectCommand = new RelayCommand(_ => StopCollecting());
            ToggleVirtualModeCommand = new RelayCommand(async _ => await ToggleVirtualModeAsync());

            ConnectionStatus = "未连接";

            // 初始化数据库
            _ = mDatabaseService.InitializeAsync();
        }
        /// <summary>
        /// 异步连接到Modbus设备
        /// </summary>
        private async Task ConnectAsync()
        {
            ConnectionStatus = "连接中...";

            if (IsVirtualMode)
            {
                // 虚拟模式：启动本地从站，连接到127.0.0.1:5020
                try
                {
                    mVirtualSlave = new VirtualModbusSlaveService(5020);
                    mVirtualSlave.Start();
                    mModbusService = new ModbusService("127.0.0.1", 5020);
                    bool success = await mModbusService.ConnectAsync();
                    ConnectionStatus = success ? "已连接 (虚拟模式 127.0.0.1:5020)" : "虚拟模式连接失败";
                }
                catch (Exception ex)
                {
                    ConnectionStatus = $"虚拟模式启动失败: {ex.Message}";
                }
            }
            else
            {
                // 真实设备模式
                mModbusService = new ModbusService(DeviceIp, DevicePort);
                bool success = await mModbusService.ConnectAsync();
                ConnectionStatus = success ? $"已连接 ({DeviceIp}:{DevicePort})" : "连接失败";
            }
        }

        /// <summary>
        /// 断开设备连接
        /// </summary>
        private void Disconnect()
        {
            StopCollecting();
            mModbusService?.Dispose();
            mVirtualSlave?.Stop();
            ConnectionStatus = "已断开";
        }

        /// <summary>
        /// 开始定时采集数据
        /// </summary>
        private async Task StartCollectingAsync()
        {
            if (mModbusService == null || !mModbusService.IsConnected)
            {
                MessageBox.Show("请先连接设备！", "提示");
                return;
            }

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
                                deviceId = DeviceIp,
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

                                // 图表最多保留50个点
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
                DeviceIp = "192.168.1.100";
                DevicePort = 502;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 停止数据采集
        /// </summary>
        private void StopCollecting()
        {
            mCancellationTokenSource?.Cancel();
            IsRunning = false;
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