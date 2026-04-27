using Modbus.Data;
using Modbus.Device;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// 虚拟Modbus TCP从站服务
    /// 在本地启动一个Modbus TCP Slave，模拟温湿度传感器
    /// 寄存器地址约定（与ModbusService一致）：
    ///   40001 (0x0000) => 温度值 × 10（整数）
    ///   40002 (0x0001) => 湿度值 × 10（整数）
    /// 
    /// 使用方法：
    ///   启动虚拟从站后，将ModbusService连接到 127.0.0.1:5020 即可读取模拟数据
    /// </summary>
    public class VirtualModbusSlaveService : IDisposable
    {
        // TCP监听器
        private TcpListener mListener;

        // Modbus从站实例
        private ModbusTcpSlave mSlave;

        // 数据寄存器（保持寄存器）
        private ushort[] mHoldingRegisters;

        // 模拟线程取消令牌
        private CancellationTokenSource mCts;

        // 随机数生成器
        private readonly Random mRandom = new Random();

        // 当前模拟温度（带波动）
        private double mCurrentTemp = 25.0;

        // 当前模拟湿度（带波动）
        private double mCurrentHumidity = 60.0;

        // 监听端口
        private readonly int mPort;

        // 从站地址
        private readonly byte mSlaveId;

        // 模拟数据更新间隔（毫秒）
        private readonly int mUpdateIntervalMs;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">监听端口，默认5020</param>
        /// <param name="slaveId">从站地址，默认1</param>
        /// <param name="updateIntervalMs">模拟数据更新间隔，默认1000ms</param>
        public VirtualModbusSlaveService(int port = 5020, byte slaveId = 1, int updateIntervalMs = 1000)
        {
            mPort = port;
            mSlaveId = slaveId;
            mUpdateIntervalMs = updateIntervalMs;
            // 分配100个寄存器空间
            mHoldingRegisters = new ushort[100];
        }

        /// <summary>
        /// 启动虚拟从站
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;

            // 先确保之前的资源已释放
            Stop();

            try
            {
                // NModbus4 的 DataStore 寄存器列表索引从1开始！
                // HoldingRegisters[0] 无效，HoldingRegisters[1] 对应 Modbus地址40001
                // 使用 DataStoreFactory.CreateDefaultDataStore() 创建，然后添加足够元素
                var dataStore = DataStoreFactory.CreateDefaultDataStore();

                // 预填充保持寄存器（索引1~100对应Modbus地址40001~40100）
                // 索引0会被NModbus忽略（列表占位），索引1开始才是有效地址
                for (int i = 0; i <= mHoldingRegisters.Length; i++)
                {
                    dataStore.HoldingRegisters.Add(0);
                }
                // 同样预填充输入寄存器
                for (int i = 0; i <= mHoldingRegisters.Length; i++)
                {
                    dataStore.InputRegisters.Add(0);
                }
                // 预填充线圈和离散输入
                for (int i = 0; i <= mHoldingRegisters.Length; i++)
                {
                    dataStore.CoilDiscretes.Add(false);
                    dataStore.InputDiscretes.Add(false);
                }

                // 创建TCP监听器，绑定到本机
                mListener = new TcpListener(IPAddress.Any, mPort);
                // 设置 SO_REUSEADDR，允许端口在关闭后快速复用
                mListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                mListener.Start(1);

                // 创建Modbus TCP从站
                mSlave = ModbusTcpSlave.CreateTcp(mSlaveId, mListener);
                mSlave.DataStore = dataStore;

                // 写入初始温湿度值
                UpdateSensorRegisters();

                // 启动从站监听（Listen会阻塞，用Task在后台运行）
                Task.Run(() => mSlave.Listen());

                // 启动模拟数据更新线程
                mCts = new CancellationTokenSource();
                Task.Run(() => SimulateSensorDataAsync(mCts.Token), mCts.Token);

                IsRunning = true;
                Console.WriteLine($"[虚拟从站] 已启动，监听端口: {mPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[虚拟从站] 启动失败: {ex.Message}");
                // 清理部分初始化的资源
                CleanupResources();
                throw;
            }
        }

        /// <summary>
        /// 停止虚拟从站
        /// </summary>
        public void Stop()
        {
            if (!IsRunning && mListener == null && mSlave == null)
                return;

            IsRunning = false;
            CleanupResources();
            Console.WriteLine("[虚拟从站] 已停止");
        }

        /// <summary>
        /// 清理底层资源（TcpListener、ModbusSlave等）
        /// 使用 SO_REUSEADDR 选项确保端口可快速复用
        /// </summary>
        private void CleanupResources()
        {
            mCts?.Cancel();
            mCts?.Dispose();
            mCts = null;

            try
            {
                mSlave?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[虚拟从站] Dispose Slave异常: {ex.Message}");
            }
            mSlave = null;

            try
            {
                mListener?.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[虚拟从站] Stop Listener异常: {ex.Message}");
            }
            mListener = null;
        }

        /// <summary>
        /// 模拟传感器数据生成
        /// 使用随机游走算法，模拟温湿度的自然波动
        /// </summary>
        private async Task SimulateSensorDataAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(mUpdateIntervalMs, token);

                    // 温度随机游走（范围 0~50℃），增大波动幅度使曲线更明显
                    double tempDelta = (mRandom.NextDouble() - 0.5) * 2.0;
                    mCurrentTemp += tempDelta;
                    mCurrentTemp = Math.Max(0, Math.Min(50, mCurrentTemp));

                    // 湿度随机游走（范围 10~99%），增大波动幅度使曲线更明显
                    double humDelta = (mRandom.NextDouble() - 0.5) * 4.0;
                    mCurrentHumidity += humDelta;
                    mCurrentHumidity = Math.Max(10, Math.Min(99, mCurrentHumidity));

                    // 更新寄存器值
                    UpdateSensorRegisters();

                    Console.WriteLine($"[虚拟从站] 温度: {mCurrentTemp:F1}℃  湿度: {mCurrentHumidity:F1}%");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[虚拟从站] 模拟异常: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 将当前温湿度值写入Modbus保持寄存器
        /// 寄存器值 = 实际值 × 10（取整数部分）
        /// 注意：NModbus4的DataStore寄存器索引从1开始！
        ///   HoldingRegisters[1] => Modbus地址40001（温度）
        ///   HoldingRegisters[2] => Modbus地址40002（湿度）
        /// </summary>
        private void UpdateSensorRegisters()
        {
            if (mSlave?.DataStore == null) return;

            // NModbus4的DataStore中寄存器索引从1开始（不是0）
            // HoldingRegisters[1] 对应 Modbus地址 40001（温度）
            // HoldingRegisters[2] 对应 Modbus地址 40002（湿度）
            mSlave.DataStore.HoldingRegisters[1] = (ushort)(mCurrentTemp * 10);
            mSlave.DataStore.HoldingRegisters[2] = (ushort)(mCurrentHumidity * 10);
        }

        /// <summary>
        /// 手动设置温度值（用于测试）
        /// </summary>
        /// <param name="temp">温度值（℃）</param>
        public void SetTemperature(double temp)
        {
            mCurrentTemp = Math.Max(0, Math.Min(50, temp));
            UpdateSensorRegisters();
        }

        /// <summary>
        /// 手动设置湿度值（用于测试）
        /// </summary>
        /// <param name="humidity">湿度值（%）</param>
        public void SetHumidity(double humidity)
        {
            mCurrentHumidity = Math.Max(10, Math.Min(99, humidity));
            UpdateSensorRegisters();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
