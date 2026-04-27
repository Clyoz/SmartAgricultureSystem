using SmartAgricultureSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SmartAgricultureSystem.Services
{
    /// <summary>
    /// SQL Server 数据库服务
    /// 负责传感器数据的持久化存储与查询、预警记录管理、系统配置管理等增删改查操作
    /// 数据库和表的创建由 Scripts/InitDatabase.sql 在 SQL Server 中手动执行
    /// 使用 ADO.NET 原生操作，兼容 .NET Framework 4.7.2
    /// </summary>
    public class DatabaseService
    {
        // 数据库连接字符串
        private readonly string mConnectionString;

        public DatabaseService()
        {
            mConnectionString = ConfigurationManager.ConnectionStrings["SmartAgricultureDB"]?.ConnectionString
                ?? "Data Source=localhost;Initial Catalog=SmartAgricultureDB;Integrated Security=True;";
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(mConnectionString);
        }

        /// <summary>
        /// 测试数据库连接是否可用
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    await conn.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #region 传感器数据操作

        /// <summary>
        /// 异步保存一条传感器数据
        /// </summary>
        public async Task SaveDataAsync(SensorData data)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO SensorData (sensorId, deviceId, greenhouseId, temperature, humidity, 
    lightIntensity, soilMoisture, co2Concentration, soilTemperature, timestamp, isAlert)
VALUES (@sensorId, @deviceId, @greenhouseId, @temperature, @humidity, 
    @lightIntensity, @soilMoisture, @co2Concentration, @soilTemperature, @timestamp, @isAlert)";
                    AddParameter(cmd, "@sensorId", data.sensorId);
                    AddParameter(cmd, "@deviceId", data.deviceId);
                    AddParameter(cmd, "@greenhouseId", data.greenhouseId);
                    AddParameter(cmd, "@temperature", data.temperature);
                    AddParameter(cmd, "@humidity", data.humidity);
                    AddParameter(cmd, "@lightIntensity", (object)data.lightIntensity ?? DBNull.Value);
                    AddParameter(cmd, "@soilMoisture", (object)data.soilMoisture ?? DBNull.Value);
                    AddParameter(cmd, "@co2Concentration", (object)data.co2Concentration ?? DBNull.Value);
                    AddParameter(cmd, "@soilTemperature", (object)data.soilTemperature ?? DBNull.Value);
                    AddParameter(cmd, "@timestamp", data.timestamp);
                    AddParameter(cmd, "@isAlert", data.isAlert);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 查询最近N条传感器数据
        /// </summary>
        public async Task<List<SensorData>> GetRecentDataAsync(int count = 100)
        {
            var result = new List<SensorData>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP (@count) id, sensorId, deviceId, greenhouseId, temperature, humidity, 
    lightIntensity, soilMoisture, co2Concentration, soilTemperature, timestamp, isAlert
FROM SensorData ORDER BY timestamp DESC";
                    AddParameter(cmd, "@count", count);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(ReadSensorData(reader));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 查询指定时间范围内的传感器数据
        /// </summary>
        public async Task<List<SensorData>> GetDataByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var result = new List<SensorData>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT id, sensorId, deviceId, greenhouseId, temperature, humidity, 
    lightIntensity, soilMoisture, co2Concentration, soilTemperature, timestamp, isAlert
FROM SensorData 
WHERE timestamp >= @startTime AND timestamp <= @endTime
ORDER BY timestamp";
                    AddParameter(cmd, "@startTime", startTime);
                    AddParameter(cmd, "@endTime", endTime);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(ReadSensorData(reader));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 删除指定时间之前的传感器数据（数据清理）
        /// </summary>
        public async Task<int> DeleteDataBeforeAsync(DateTime beforeTime)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM SensorData WHERE timestamp < @beforeTime";
                    AddParameter(cmd, "@beforeTime", beforeTime);
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 从DataReader读取SensorData对象
        /// </summary>
        private SensorData ReadSensorData(SqlDataReader reader)
        {
            return new SensorData
            {
                id = (int)reader["id"],
                sensorId = (int)reader["sensorId"],
                deviceId = (int)reader["deviceId"],
                greenhouseId = (int)reader["greenhouseId"],
                temperature = Convert.ToDouble(reader["temperature"]),
                humidity = Convert.ToDouble(reader["humidity"]),
                lightIntensity = reader["lightIntensity"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["lightIntensity"]),
                soilMoisture = reader["soilMoisture"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["soilMoisture"]),
                co2Concentration = reader["co2Concentration"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["co2Concentration"]),
                soilTemperature = reader["soilTemperature"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["soilTemperature"]),
                timestamp = (DateTime)reader["timestamp"],
                isAlert = (bool)reader["isAlert"]
            };
        }

        #endregion

        #region 预警记录操作

        /// <summary>
        /// 保存预警记录
        /// </summary>
        public async Task SaveAlertRecordAsync(AlertRecord record)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO AlertRecords (ruleId, sensorDataId, greenhouseId, deviceId, sensorId, 
    alertType, alertLevel, triggerValue, thresholdValue, message)
VALUES (@ruleId, @sensorDataId, @greenhouseId, @deviceId, @sensorId, 
    @alertType, @alertLevel, @triggerValue, @thresholdValue, @message)";
                    AddParameter(cmd, "@ruleId", (object)record.ruleId ?? DBNull.Value);
                    AddParameter(cmd, "@sensorDataId", (object)record.sensorDataId ?? DBNull.Value);
                    AddParameter(cmd, "@greenhouseId", record.greenhouseId);
                    AddParameter(cmd, "@deviceId", record.deviceId);
                    AddParameter(cmd, "@sensorId", record.sensorId);
                    AddParameter(cmd, "@alertType", record.alertType);
                    AddParameter(cmd, "@alertLevel", record.alertLevel);
                    AddParameter(cmd, "@triggerValue", record.triggerValue);
                    AddParameter(cmd, "@thresholdValue", record.thresholdValue);
                    AddParameter(cmd, "@message", (object)record.message ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 查询未处理的预警记录
        /// </summary>
        public async Task<List<AlertRecord>> GetUnhandledAlertsAsync()
        {
            var result = new List<AlertRecord>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 50 id, ruleId, sensorDataId, greenhouseId, deviceId, sensorId,
    alertType, alertLevel, triggerValue, thresholdValue, message, 
    handleStatus, handlerId, handleTime, handleRemark, createdAt
FROM AlertRecords 
WHERE handleStatus = 0 
ORDER BY createdAt DESC";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new AlertRecord
                            {
                                id = (int)reader["id"],
                                ruleId = reader["ruleId"] == DBNull.Value ? (int?)null : (int)reader["ruleId"],
                                sensorDataId = reader["sensorDataId"] == DBNull.Value ? (int?)null : (int)reader["sensorDataId"],
                                greenhouseId = (int)reader["greenhouseId"],
                                deviceId = (int)reader["deviceId"],
                                sensorId = (int)reader["sensorId"],
                                alertType = (int)reader["alertType"],
                                alertLevel = (int)reader["alertLevel"],
                                triggerValue = Convert.ToDouble(reader["triggerValue"]),
                                thresholdValue = Convert.ToDouble(reader["thresholdValue"]),
                                message = reader["message"]?.ToString(),
                                handleStatus = (int)reader["handleStatus"],
                                handlerId = reader["handlerId"] == DBNull.Value ? (int?)null : (int)reader["handlerId"],
                                handleTime = reader["handleTime"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["handleTime"],
                                handleRemark = reader["handleRemark"]?.ToString(),
                                createdAt = (DateTime)reader["createdAt"]
                            });
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 处理预警记录（标记为已处理）
        /// </summary>
        public async Task HandleAlertAsync(int alertId, int handlerId, string handleRemark)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE AlertRecords 
SET handleStatus = 1, handlerId = @handlerId, handleTime = GETDATE(), handleRemark = @handleRemark
WHERE id = @id";
                    AddParameter(cmd, "@handlerId", handlerId);
                    AddParameter(cmd, "@handleRemark", (object)handleRemark ?? DBNull.Value);
                    AddParameter(cmd, "@id", alertId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region 系统配置操作

        /// <summary>
        /// 获取系统配置值
        /// </summary>
        public async Task<string> GetConfigValueAsync(string key)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT configValue FROM SystemConfig WHERE configKey = @key AND isEnabled = 1";
                    AddParameter(cmd, "@key", key);
                    var val = await cmd.ExecuteScalarAsync();
                    return val?.ToString();
                }
            }
        }

        /// <summary>
        /// 设置系统配置值
        /// </summary>
        public async Task SetConfigValueAsync(string key, string value, string group = null, string description = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM SystemConfig WHERE configKey = @key)
    UPDATE SystemConfig SET configValue = @value, updatedAt = GETDATE() WHERE configKey = @key
ELSE
    INSERT INTO SystemConfig (configKey, configValue, configGroup, description) VALUES (@key, @value, @group, @desc)";
                    AddParameter(cmd, "@key", key);
                    AddParameter(cmd, "@value", value);
                    AddParameter(cmd, "@group", (object)group ?? DBNull.Value);
                    AddParameter(cmd, "@desc", (object)description ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region 设备操作

        /// <summary>
        /// 查询所有传感器节点设备（deviceType=2）
        /// </summary>
        public async Task<List<Device>> GetSensorDevicesAsync()
        {
            var result = new List<Device>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT id, deviceCode, name, greenhouseId, deviceType, ipAddress, port, slaveId, 
    model, firmwareVersion, isOnline, lastOnlineTime, installDate, remark, createdAt, updatedAt
FROM Devices 
WHERE deviceType = 2
ORDER BY id";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new Device
                            {
                                id = (int)reader["id"],
                                deviceCode = reader["deviceCode"]?.ToString(),
                                name = reader["name"]?.ToString(),
                                greenhouseId = (int)reader["greenhouseId"],
                                deviceType = (int)reader["deviceType"],
                                ipAddress = reader["ipAddress"]?.ToString(),
                                port = (int)reader["port"],
                                slaveId = (byte)reader["slaveId"],
                                model = reader["model"]?.ToString(),
                                firmwareVersion = reader["firmwareVersion"]?.ToString(),
                                isOnline = (bool)reader["isOnline"],
                                lastOnlineTime = reader["lastOnlineTime"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["lastOnlineTime"],
                                installDate = reader["installDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["installDate"],
                                remark = reader["remark"]?.ToString(),
                                createdAt = (DateTime)reader["createdAt"],
                                updatedAt = reader["updatedAt"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["updatedAt"]
                            });
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 更新设备在线状态
        /// </summary>
        public async Task UpdateDeviceOnlineStatusAsync(int deviceId, bool isOnline)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE Devices SET isOnline = @isOnline, lastOnlineTime = CASE WHEN @isOnline=1 THEN GETDATE() ELSE lastOnlineTime END, updatedAt = GETDATE()
WHERE id = @id";
                    AddParameter(cmd, "@isOnline", isOnline);
                    AddParameter(cmd, "@id", deviceId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region 通用辅助方法

        /// <summary>
        /// 添加SQL参数
        /// </summary>
        private void AddParameter(SqlCommand cmd, string name, object value)
        {
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        /// <summary>
        /// 执行非查询SQL（用于扩展操作）
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 执行查询并返回DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        await Task.Run(() => adapter.Fill(dt));
                        return dt;
                    }
                }
            }
        }

        #endregion
    }
}
