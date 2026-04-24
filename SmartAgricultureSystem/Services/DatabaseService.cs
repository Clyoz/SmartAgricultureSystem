using SmartAgricultureSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;


namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// SQLite 数据库服务
    /// 负责传感器数据的持久化存储与查询
    /// </summary>
    public class DatabaseService
    {
        // 数据库连接实例（异步版本）
        private SQLiteAsyncConnection mConnection;

        // 数据库文件路径
        private static readonly string DB_PATH =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AgricultureData.db");

        /// <summary>
        /// 初始化数据库，创建数据表
        /// </summary>
        public async Task InitializeAsync()
        {
            mConnection = new SQLiteAsyncConnection(DB_PATH);
            // 自动创建SensorData表（如果不存在）
            await mConnection.CreateTableAsync<SensorData>();
        }

        /// <summary>
        /// 异步保存一条传感器数据
        /// </summary>
        /// <param name="data">传感器数据对象</param>
        public async Task SaveDataAsync(SensorData data)
        {
            await mConnection.InsertAsync(data);
        }

        /// <summary>
        /// 查询最近N条数据记录
        /// </summary>
        /// <param name="count">查询条数</param>
        /// <returns>传感器数据列表</returns>
        public async Task<List<SensorData>> GetRecentDataAsync(int count = 100)
        {
            return await mConnection.Table<SensorData>()
                .OrderByDescending(d => d.timestamp)
                .Take(count)
                .ToListAsync();
        }
        /// <summary>
        /// 查询指定时间范围内的数据
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>传感器数据列表</returns>
        public async Task<List<SensorData>> GetDataByTimeRangeAsync(
            DateTime startTime, DateTime endTime)
        {
            return await mConnection.Table<SensorData>()
                .Where(d => d.timestamp >= startTime && d.timestamp <= endTime)
                .OrderBy(d => d.timestamp)
                .ToListAsync();
        }
    }

}
