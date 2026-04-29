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

        #region 蔬菜管理操作

        /// <summary>
        /// 获取所有作物信息
        /// </summary>
        public async Task<List<CropInfo>> GetAllCropsAsync()
        {
            var result = new List<CropInfo>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM CropInfo ORDER BY id";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new CropInfo
                            {
                                id = (int)reader["id"],
                                cropName = reader["cropName"]?.ToString(),
                                variety = reader["variety"]?.ToString(),
                                tempMin = Convert.ToDouble(reader["tempMin"]),
                                tempMax = Convert.ToDouble(reader["tempMax"]),
                                humidityMin = Convert.ToDouble(reader["humidityMin"]),
                                humidityMax = Convert.ToDouble(reader["humidityMax"]),
                                lightMin = reader["lightMin"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["lightMin"]),
                                lightMax = reader["lightMax"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["lightMax"]),
                                growthCycleDays = (int)reader["growthCycleDays"],
                                description = reader["description"]?.ToString(),
                                createdAt = (DateTime)reader["createdAt"]
                            });
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 添加作物信息
        /// </summary>
        public async Task<int> AddCropAsync(CropInfo crop)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO CropInfo (cropName, variety, tempMin, tempMax, humidityMin, humidityMax, lightMin, lightMax, growthCycleDays, description)
VALUES (@cropName, @variety, @tempMin, @tempMax, @humidityMin, @humidityMax, @lightMin, @lightMax, @growthCycleDays, @description);
SELECT SCOPE_IDENTITY();";
                    AddParameter(cmd, "@cropName", crop.cropName);
                    AddParameter(cmd, "@variety", (object)crop.variety ?? DBNull.Value);
                    AddParameter(cmd, "@tempMin", crop.tempMin);
                    AddParameter(cmd, "@tempMax", crop.tempMax);
                    AddParameter(cmd, "@humidityMin", crop.humidityMin);
                    AddParameter(cmd, "@humidityMax", crop.humidityMax);
                    AddParameter(cmd, "@lightMin", (object)crop.lightMin ?? DBNull.Value);
                    AddParameter(cmd, "@lightMax", (object)crop.lightMax ?? DBNull.Value);
                    AddParameter(cmd, "@growthCycleDays", crop.growthCycleDays);
                    AddParameter(cmd, "@description", (object)crop.description ?? DBNull.Value);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        /// <summary>
        /// 更新作物信息
        /// </summary>
        public async Task UpdateCropAsync(CropInfo crop)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE CropInfo SET cropName=@cropName, variety=@variety, tempMin=@tempMin, tempMax=@tempMax,
    humidityMin=@humidityMin, humidityMax=@humidityMax, lightMin=@lightMin, lightMax=@lightMax,
    growthCycleDays=@growthCycleDays, description=@description
WHERE id=@id";
                    AddParameter(cmd, "@cropName", crop.cropName);
                    AddParameter(cmd, "@variety", (object)crop.variety ?? DBNull.Value);
                    AddParameter(cmd, "@tempMin", crop.tempMin);
                    AddParameter(cmd, "@tempMax", crop.tempMax);
                    AddParameter(cmd, "@humidityMin", crop.humidityMin);
                    AddParameter(cmd, "@humidityMax", crop.humidityMax);
                    AddParameter(cmd, "@lightMin", (object)crop.lightMin ?? DBNull.Value);
                    AddParameter(cmd, "@lightMax", (object)crop.lightMax ?? DBNull.Value);
                    AddParameter(cmd, "@growthCycleDays", crop.growthCycleDays);
                    AddParameter(cmd, "@description", (object)crop.description ?? DBNull.Value);
                    AddParameter(cmd, "@id", crop.id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 删除作物信息
        /// </summary>
        public async Task DeleteCropAsync(int cropId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM CropInfo WHERE id=@id";
                    AddParameter(cmd, "@id", cropId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region 大棚管理操作

        /// <summary>
        /// 获取所有大棚
        /// </summary>
        public async Task<List<Greenhouse>> GetAllGreenhousesAsync()
        {
            var result = new List<Greenhouse>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Greenhouses ORDER BY id";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new Greenhouse
                            {
                                id = (int)reader["id"],
                                greenhouseCode = reader["greenhouseCode"]?.ToString(),
                                name = reader["name"]?.ToString(),
                                location = reader["location"]?.ToString(),
                                area = reader["area"] == DBNull.Value ? 0 : Convert.ToDouble(reader["area"]),
                                greenhouseType = reader["greenhouseType"]?.ToString(),
                                managerId = reader["managerId"] == DBNull.Value ? (int?)null : (int)reader["managerId"],
                                buildDate = reader["buildDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["buildDate"],
                                status = (int)reader["status"],
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
        /// 添加大棚
        /// </summary>
        public async Task<int> AddGreenhouseAsync(Greenhouse gh)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO Greenhouses (greenhouseCode, name, location, area, greenhouseType, managerId, buildDate, status, remark)
VALUES (@greenhouseCode, @name, @location, @area, @greenhouseType, @managerId, @buildDate, @status, @remark);
SELECT SCOPE_IDENTITY();";
                    AddParameter(cmd, "@greenhouseCode", gh.greenhouseCode);
                    AddParameter(cmd, "@name", gh.name);
                    AddParameter(cmd, "@location", (object)gh.location ?? DBNull.Value);
                    AddParameter(cmd, "@area", gh.area);
                    AddParameter(cmd, "@greenhouseType", (object)gh.greenhouseType ?? DBNull.Value);
                    AddParameter(cmd, "@managerId", (object)gh.managerId ?? DBNull.Value);
                    AddParameter(cmd, "@buildDate", (object)gh.buildDate ?? DBNull.Value);
                    AddParameter(cmd, "@status", gh.status);
                    AddParameter(cmd, "@remark", (object)gh.remark ?? DBNull.Value);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        /// <summary>
        /// 更新大棚
        /// </summary>
        public async Task UpdateGreenhouseAsync(Greenhouse gh)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE Greenhouses SET greenhouseCode=@greenhouseCode, name=@name, location=@location, area=@area,
    greenhouseType=@greenhouseType, managerId=@managerId, buildDate=@buildDate, status=@status,
    remark=@remark, updatedAt=GETDATE()
WHERE id=@id";
                    AddParameter(cmd, "@greenhouseCode", gh.greenhouseCode);
                    AddParameter(cmd, "@name", gh.name);
                    AddParameter(cmd, "@location", (object)gh.location ?? DBNull.Value);
                    AddParameter(cmd, "@area", gh.area);
                    AddParameter(cmd, "@greenhouseType", (object)gh.greenhouseType ?? DBNull.Value);
                    AddParameter(cmd, "@managerId", (object)gh.managerId ?? DBNull.Value);
                    AddParameter(cmd, "@buildDate", (object)gh.buildDate ?? DBNull.Value);
                    AddParameter(cmd, "@status", gh.status);
                    AddParameter(cmd, "@remark", (object)gh.remark ?? DBNull.Value);
                    AddParameter(cmd, "@id", gh.id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 删除大棚
        /// </summary>
        public async Task DeleteGreenhouseAsync(int greenhouseId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Greenhouses WHERE id=@id";
                    AddParameter(cmd, "@id", greenhouseId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region 设备管理操作

        /// <summary>
        /// 获取所有设备
        /// </summary>
        public async Task<List<Device>> GetAllDevicesAsync()
        {
            var result = new List<Device>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Devices ORDER BY id";
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
        /// 添加设备
        /// </summary>
        public async Task<int> AddDeviceAsync(Device device)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO Devices (deviceCode, name, greenhouseId, deviceType, ipAddress, port, slaveId, model, firmwareVersion, isOnline, installDate, remark)
VALUES (@deviceCode, @name, @greenhouseId, @deviceType, @ipAddress, @port, @slaveId, @model, @firmwareVersion, @isOnline, @installDate, @remark);
SELECT SCOPE_IDENTITY();";
                    AddParameter(cmd, "@deviceCode", device.deviceCode);
                    AddParameter(cmd, "@name", device.name);
                    AddParameter(cmd, "@greenhouseId", device.greenhouseId);
                    AddParameter(cmd, "@deviceType", device.deviceType);
                    AddParameter(cmd, "@ipAddress", (object)device.ipAddress ?? DBNull.Value);
                    AddParameter(cmd, "@port", device.port);
                    AddParameter(cmd, "@slaveId", device.slaveId);
                    AddParameter(cmd, "@model", (object)device.model ?? DBNull.Value);
                    AddParameter(cmd, "@firmwareVersion", (object)device.firmwareVersion ?? DBNull.Value);
                    AddParameter(cmd, "@isOnline", device.isOnline);
                    AddParameter(cmd, "@installDate", (object)device.installDate ?? DBNull.Value);
                    AddParameter(cmd, "@remark", (object)device.remark ?? DBNull.Value);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        /// <summary>
        /// 更新设备
        /// </summary>
        public async Task UpdateDeviceAsync(Device device)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE Devices SET deviceCode=@deviceCode, name=@name, greenhouseId=@greenhouseId, deviceType=@deviceType,
    ipAddress=@ipAddress, port=@port, slaveId=@slaveId, model=@model, firmwareVersion=@firmwareVersion,
    installDate=@installDate, remark=@remark, updatedAt=GETDATE()
WHERE id=@id";
                    AddParameter(cmd, "@deviceCode", device.deviceCode);
                    AddParameter(cmd, "@name", device.name);
                    AddParameter(cmd, "@greenhouseId", device.greenhouseId);
                    AddParameter(cmd, "@deviceType", device.deviceType);
                    AddParameter(cmd, "@ipAddress", (object)device.ipAddress ?? DBNull.Value);
                    AddParameter(cmd, "@port", device.port);
                    AddParameter(cmd, "@slaveId", device.slaveId);
                    AddParameter(cmd, "@model", (object)device.model ?? DBNull.Value);
                    AddParameter(cmd, "@firmwareVersion", (object)device.firmwareVersion ?? DBNull.Value);
                    AddParameter(cmd, "@installDate", (object)device.installDate ?? DBNull.Value);
                    AddParameter(cmd, "@remark", (object)device.remark ?? DBNull.Value);
                    AddParameter(cmd, "@id", device.id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        public async Task DeleteDeviceAsync(int deviceId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Devices WHERE id=@id";
                    AddParameter(cmd, "@id", deviceId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region 人员管理与登录日志操作

        /// <summary>
        /// 更新用户信息（昵称、角色、手机、邮箱）
        /// </summary>
        public async Task UpdateUserAsync(int userId, string nickname, UserRole role, string phone, string email)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE Users SET nickname=@nickname, role=@role, phoneNumber=@phone, email=@email WHERE id=@id";
                    AddParameter(cmd, "@nickname", (object)nickname ?? DBNull.Value);
                    AddParameter(cmd, "@role", (int)role);
                    AddParameter(cmd, "@phone", (object)phone ?? DBNull.Value);
                    AddParameter(cmd, "@email", (object)email ?? DBNull.Value);
                    AddParameter(cmd, "@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        public async Task DeleteUserAsync(int userId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Users WHERE id=@id";
                    AddParameter(cmd, "@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 重置用户密码
        /// </summary>
        public async Task ResetUserPasswordAsync(int userId, string passwordHash, string passwordSalt)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET passwordHash=@hash, passwordSalt=@salt, isLocked=0, failedLoginCount=0, lockUntil=NULL WHERE id=@id";
                    AddParameter(cmd, "@hash", passwordHash);
                    AddParameter(cmd, "@salt", passwordSalt);
                    AddParameter(cmd, "@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 获取最近N条登录记录
        /// </summary>
        public async Task<List<LoginRecord>> GetRecentLoginRecordsAsync(int count = 50)
        {
            var result = new List<LoginRecord>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP (@count) id, username, userId, loginTime, isSuccess, failReason, ipAddress, deviceInfo
FROM LoginRecords ORDER BY loginTime DESC";
                    AddParameter(cmd, "@count", count);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new LoginRecord
                            {
                                id = (int)reader["id"],
                                username = reader["username"]?.ToString(),
                                userId = reader["userId"] == DBNull.Value ? (int?)null : (int)reader["userId"],
                                loginTime = (DateTime)reader["loginTime"],
                                isSuccess = (bool)reader["isSuccess"],
                                failReason = reader["failReason"]?.ToString(),
                                ipAddress = reader["ipAddress"]?.ToString(),
                                deviceInfo = reader["deviceInfo"]?.ToString()
                            });
                        }
                    }
                }
            }
            return result;
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
