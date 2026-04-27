using Modbus.Device;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// Modbus TCP 通讯服务
    /// 负责与传感器设备进行数据交互
    /// 寄存器地址约定（Modbus协议地址，从0开始）：
    ///   地址0 (40001) => 温度值 × 10（整数）
    ///   地址1 (40002) => 湿度值 × 10（整数）
    /// 
    /// 注意：NModbus4 的 ReadHoldingRegistersAsync 使用 Modbus协议地址（从0开始），
    ///   协议地址0 对应 DataStore.HoldingRegisters[1]（DataStore索引从1开始）
    ///   协议地址1 对应 DataStore.HoldingRegisters[2]
    ///   两者之间偏移量为1
    /// </summary>
    public class ModbusService : IDisposable
    {
        // TCP客户端
        private TcpClient mTcpClient;

        // Modbus主站实例
        private ModbusIpMaster mMaster;

        // 目标IP地址
        private readonly string mIpAddress;

        // 目标端口号
        private readonly int mPort;

        // 从站地址（设备ID）
        private readonly byte mSlaveId;

        // 连接状态标志
        private bool mIsConnected;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => mIsConnected;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">设备IP地址</param>
        /// <param name="port">端口号，默认502</param>
        /// <param name="slaveId">从站地址，默认1</param>
        public ModbusService(string ipAddress, int port = 502, byte slaveId = 1)
        {
            mIpAddress = ipAddress;
            mPort = port;
            mSlaveId = slaveId;
        }
        /// <summary>
        /// 异步连接到Modbus设备
        /// </summary>
        /// <returns>连接是否成功</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                mTcpClient = new TcpClient();
                // 设置连接超时为3秒
                var connectTask = mTcpClient.ConnectAsync(mIpAddress, mPort);
                if (await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(3))) == connectTask)
                {
                    // 连接成功
                    mMaster = ModbusIpMaster.CreateIp(mTcpClient);
                    mIsConnected = true;
                    return true;
                }
                else
                {
                    // 超时
                    mIsConnected = false;
                    Console.WriteLine("[Modbus] 连接超时");
                    return false;
                }
            }
            catch (Exception ex)
            {
                mIsConnected = false;
                Console.WriteLine($"[Modbus] 连接失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 异步读取温度值
        /// 读取保持寄存器协议地址0（对应Modbus地址40001），返回实际温度（除以10）
        /// </summary>
        /// <returns>温度值（℃），失败返回 double.NaN</returns>
        public async Task<double> ReadTemperatureAsync()
        {
            try
            {
                // 协议地址0 => DataStore.HoldingRegisters[1] => 40001(温度)
                ushort[] registers = await mMaster.ReadHoldingRegistersAsync(mSlaveId, 0, 1);
                return registers[0] / 10.0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Modbus] 读取温度失败: {ex.Message}");
                return double.NaN;
            }
        }

        /// <summary>
        /// 异步读取湿度值
        /// 读取保持寄存器协议地址1（对应Modbus地址40002），返回实际湿度（除以10）
        /// </summary>
        /// <returns>湿度值（%RH），失败返回 double.NaN</returns>
        public async Task<double> ReadHumidityAsync()
        {
            try
            {
                // 协议地址1 => DataStore.HoldingRegisters[2] => 40002(湿度)
                ushort[] registers = await mMaster.ReadHoldingRegistersAsync(mSlaveId, 1, 1);
                return registers[0] / 10.0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Modbus] 读取湿度失败: {ex.Message}");
                return double.NaN;
            }
        }

        /// <summary>
        /// 同时读取温度和湿度（一次性读取2个寄存器，减少通讯次数）
        /// 从协议地址0开始连续读取2个寄存器
        /// </summary>
        /// <returns>温湿度元组 (温度, 湿度)</returns>
        public async Task<(double temperature, double humidity)> ReadTempHumidityAsync()
        {
            try
            {
                // 协议地址0 => DataStore[1]=40001(温度), 协议地址1 => DataStore[2]=40002(湿度)
                ushort[] registers = await mMaster.ReadHoldingRegistersAsync(mSlaveId, 0, 2);
                double temperature = registers[0] / 10.0;
                double humidity = registers[1] / 10.0;
                return (temperature, humidity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Modbus] 批量读取失败: {ex.Message}");
                return (double.NaN, double.NaN);
            }
        }

        /// <summary>
        /// 断开连接并释放资源
        /// </summary>
        public void Dispose()
        {
            mIsConnected = false;
            mMaster?.Dispose();
            mTcpClient?.Close();
        }
    }
}