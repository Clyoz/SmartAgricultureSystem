-- ============================================================
-- 智慧农业系统 - SQL Server 数据库初始化脚本（修正版）
-- 请在 SQL Server Management Studio (SSMS) 中执行此脚本
-- ⚠ 如果数据库已存在且需要重新初始化，请先执行以下语句删除旧库：
--    DROP DATABASE SmartAgricultureDB;
-- ============================================================
-- 1. 创建数据库（如果已存在可跳过此步）
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SmartAgricultureDB')
BEGIN
    CREATE DATABASE SmartAgricultureDB;
END
GO
USE SmartAgricultureDB;
GO

-- 2. 用户表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    username        NVARCHAR(50) NOT NULL UNIQUE,
    passwordHash    NVARCHAR(256) NOT NULL,
    passwordSalt    NVARCHAR(128) NOT NULL,
    nickname        NVARCHAR(50),
    phoneNumber     NVARCHAR(20),
    email           NVARCHAR(100),
    avatarPath      NVARCHAR(500),
    role            INT NOT NULL DEFAULT 2,
    isLocked        BIT NOT NULL DEFAULT 0,
    failedLoginCount INT NOT NULL DEFAULT 0,
    lockUntil       DATETIME NULL,
    createdAt       DATETIME NOT NULL DEFAULT GETDATE(),
    lastLoginAt     DATETIME NULL,
    rememberLogin   BIT NOT NULL DEFAULT 0,
    rememberToken   NVARCHAR(128) NULL,
    tokenExpireAt   DATETIME NULL,
    remark          NVARCHAR(500)
);
GO

-- 3. 大棚表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Greenhouses')
CREATE TABLE Greenhouses (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    greenhouseCode  NVARCHAR(20) NOT NULL UNIQUE,
    name            NVARCHAR(50) NOT NULL,
    location        NVARCHAR(200),
    area            FLOAT DEFAULT 0,
    greenhouseType  NVARCHAR(50),
    managerId       INT NULL, -- 先暂时去掉外键约束，后面再添加
    buildDate       DATETIME NULL,
    status          INT NOT NULL DEFAULT 1,
    remark          NVARCHAR(500),
    createdAt       DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt       DATETIME NULL
);
GO

-- 4. 设备表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Devices')
CREATE TABLE Devices (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    deviceCode      NVARCHAR(20) NOT NULL UNIQUE,
    name            NVARCHAR(50) NOT NULL,
    greenhouseId    INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    deviceType      INT NOT NULL DEFAULT 2,
    ipAddress       NVARCHAR(50),
    port            INT DEFAULT 502,
    slaveId         TINYINT DEFAULT 1,
    model           NVARCHAR(50),
    firmwareVersion NVARCHAR(50),
    isOnline        BIT NOT NULL DEFAULT 0,
    lastOnlineTime  DATETIME NULL,
    installDate     DATETIME NULL,
    remark          NVARCHAR(500),
    createdAt       DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt       DATETIME NULL
);
GO

-- 5. 传感器表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sensors')
CREATE TABLE Sensors (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    sensorCode      NVARCHAR(20) NOT NULL UNIQUE,
    name            NVARCHAR(50) NOT NULL,
    deviceId        INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    sensorType      INT NOT NULL,
    registerAddress SMALLINT NOT NULL DEFAULT 0,
    registerCount   INT DEFAULT 1,
    unit            NVARCHAR(20),
    minValue        FLOAT DEFAULT 0,
    maxValue        FLOAT DEFAULT 100,
    precisionFactor FLOAT DEFAULT 0.1,
    pollIntervalMs  INT DEFAULT 2000,
    isEnabled       BIT NOT NULL DEFAULT 1,
    remark          NVARCHAR(500),
    createdAt       DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 6. 作物信息表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CropInfo')
CREATE TABLE CropInfo (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    cropName        NVARCHAR(50) NOT NULL,
    variety         NVARCHAR(50),
    tempMin         FLOAT NOT NULL DEFAULT 0,
    tempMax         FLOAT NOT NULL DEFAULT 50,
    humidityMin     FLOAT NOT NULL DEFAULT 0,
    humidityMax     FLOAT NOT NULL DEFAULT 100,
    lightMin        FLOAT NULL,
    lightMax        FLOAT NULL,
    growthCycleDays INT DEFAULT 90,
    description     NVARCHAR(1000),
    createdAt       DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 7. 大棚作物关联表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GreenhouseCrops')
CREATE TABLE GreenhouseCrops (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    greenhouseId        INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    cropId              INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    plantingArea        FLOAT DEFAULT 0,
    plantingDate        DATETIME NOT NULL,
    expectedHarvestDate DATETIME NULL,
    actualHarvestDate   DATETIME NULL,
    growthStage         INT NOT NULL DEFAULT 1,
    status              INT NOT NULL DEFAULT 1,
    managerId           INT NULL, -- 先暂时去掉外键约束，后面再添加
    remark              NVARCHAR(500),
    createdAt           DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt           DATETIME NULL
);
GO

-- 8. 预警规则表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AlertRules')
CREATE TABLE AlertRules (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    ruleName    NVARCHAR(100) NOT NULL,
    sensorType  INT NOT NULL,
    greenhouseId INT NULL, -- 先暂时去掉外键约束，后面再添加
    minValue    FLOAT NULL,
    maxValue    FLOAT NULL,
    alertLevel  INT NOT NULL DEFAULT 2,
    isEnabled   BIT NOT NULL DEFAULT 1,
    remark      NVARCHAR(500),
    createdAt   DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt   DATETIME NULL
);
GO

-- 9. 传感器采集数据表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SensorData')
CREATE TABLE SensorData (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    sensorId        INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    deviceId        INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    greenhouseId    INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    temperature     FLOAT NOT NULL DEFAULT 0,
    humidity        FLOAT NOT NULL DEFAULT 0,
    lightIntensity  FLOAT NULL,
    soilMoisture    FLOAT NULL,
    co2Concentration FLOAT NULL,
    soilTemperature FLOAT NULL,
    timestamp       DATETIME NOT NULL DEFAULT GETDATE(),
    isAlert         BIT NOT NULL DEFAULT 0
);
GO

-- 10. 预警记录表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AlertRecords')
CREATE TABLE AlertRecords (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ruleId          INT NULL, -- 先暂时去掉外键约束，后面再添加
    sensorDataId    INT NULL, -- 先暂时去掉外键约束，后面再添加
    greenhouseId    INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    deviceId        INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    sensorId        INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    alertType       INT NOT NULL,
    alertLevel      INT NOT NULL DEFAULT 2,
    triggerValue    FLOAT NOT NULL,
    thresholdValue  FLOAT NOT NULL,
    message         NVARCHAR(500),
    handleStatus    INT NOT NULL DEFAULT 0,
    handlerId       INT NULL, -- 先暂时去掉外键约束，后面再添加
    handleTime      DATETIME NULL,
    handleRemark    NVARCHAR(500),
    createdAt       DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 11. 控制指令表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ControlCommands')
CREATE TABLE ControlCommands (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    deviceId        INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    greenhouseId    INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    commandType     INT NOT NULL,
    commandParam    NVARCHAR(200),
    status          INT NOT NULL DEFAULT 0,
    userId          INT NOT NULL, -- 先暂时去掉外键约束，后面再添加
    sendType        INT NOT NULL DEFAULT 1,
    resultMessage   NVARCHAR(500),
    sendTime        DATETIME NOT NULL DEFAULT GETDATE(),
    executeTime     DATETIME NULL,
    remark          NVARCHAR(500)
);
GO

-- 12. 系统配置表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemConfig')
CREATE TABLE SystemConfig (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    configKey   NVARCHAR(100) NOT NULL UNIQUE,
    configValue NVARCHAR(500),
    configGroup NVARCHAR(50),
    description NVARCHAR(500),
    isEnabled   BIT NOT NULL DEFAULT 1,
    createdAt   DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt   DATETIME NULL
);
GO

-- 13. 登录记录表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginRecords')
CREATE TABLE LoginRecords (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    username    NVARCHAR(50) NOT NULL,
    userId      INT NULL, -- 先暂时去掉外键约束，后面再添加
    loginTime   DATETIME NOT NULL DEFAULT GETDATE(),
    isSuccess   BIT NOT NULL,
    failReason  NVARCHAR(200),
    ipAddress   NVARCHAR(50),
    deviceInfo  NVARCHAR(200) DEFAULT 'Windows PC'
);
GO

-- ============================================================
-- 插入虚拟种子数据
-- 使用 SET IDENTITY_INSERT ON 显式指定ID，确保外键引用一致
-- 密码统一为 Admin@123456，算法: SHA256(密码 + 盐值)
-- ============================================================

-- ========== 1. 用户表（2条：id=1 管理员, id=2 农户） ==========
-- role: 1=管理员, 2=农户, 3=技术人员
-- 密码统一为 Admin@123456，算法: SHA256(密码 + 盐值)，盐值: InitSalt2024SmartAgri
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (id, username, passwordHash, passwordSalt, nickname, phoneNumber, email, role, isLocked, failedLoginCount, createdAt, remark)
VALUES 
(1, 'admin',    '8df607e023e01c61783cc53fa38be279d274ee4026d2b126208adc352720d2a1', 'InitSalt2024SmartAgri', N'系统管理员', NULL, NULL, 1, 0, 0, GETDATE(), N'系统默认管理员'),
(2, 'farmer01', '8df607e023e01c61783cc53fa38be279d274ee4026d2b126208adc352720d2a1', 'InitSalt2024SmartAgri', N'张农民', '13800001111', 'farmer01@agri.com', 2, 0, 0, GETDATE(), N'普通农户账号');
SET IDENTITY_INSERT Users OFF;
GO

-- ========== 2. 大棚表（3条） ==========
-- status: 1=空闲, 2=种植中, 3=维护中
SET IDENTITY_INSERT Greenhouses ON;
INSERT INTO Greenhouses (id, greenhouseCode, name, location, area, greenhouseType, managerId, buildDate, status, remark)
VALUES 
(1, 'GH-001', N'1号番茄大棚', N'东区A栋', 1200, N'玻璃温室', 2, '2024-03-15', 2, N'种植番茄中'),
(2, 'GH-002', N'2号草莓大棚', N'东区B栋', 800,  N'塑料大棚', 2, '2024-05-20', 2, N'种植草莓中'),
(3, 'GH-003', N'3号育苗大棚', N'西区C栋', 600,  N'连栋温室', 1, '2023-11-10', 1, N'当前空闲，准备下季育苗');
SET IDENTITY_INSERT Greenhouses OFF;
GO

-- ========== 3. 设备表（4条） ==========
-- deviceType: 1=网关, 2=传感器节点, 3=控制器
SET IDENTITY_INSERT Devices ON;
INSERT INTO Devices (id, deviceCode, name, greenhouseId, deviceType, ipAddress, port, slaveId, model, firmwareVersion, isOnline, installDate, remark)
VALUES 
(1, 'DEV-GW01', N'1号网关',       1, 1, '192.168.1.101', 502, 1, N'AG-GW200', 'V2.1.0', 1, '2024-04-01', N'1号大棚主网关'),
(2, 'DEV-SN01', N'1号传感器节点', 1, 2, '192.168.1.102', 502, 1, N'AG-SN100', 'V1.5.3', 1, '2024-04-01', N'温湿度光照传感器组'),
(3, 'DEV-GW02', N'2号网关',       2, 1, '192.168.1.201', 502, 1, N'AG-GW200', 'V2.1.0', 1, '2024-06-01', N'2号大棚主网关'),
(4, 'DEV-CT01', N'1号控制器',     1, 3, '192.168.1.103', 502, 2, N'AG-CT300', 'V1.2.0', 0, '2024-04-01', N'通风+灌溉控制器');
SET IDENTITY_INSERT Devices OFF;
GO

-- ========== 4. 传感器表（8条） ==========
-- sensorType: 1=温度, 2=湿度, 3=光照, 4=土壤湿度, 5=CO2浓度, 6=土壤温度
SET IDENTITY_INSERT Sensors ON;
INSERT INTO Sensors (id, sensorCode, name, deviceId, sensorType, registerAddress, registerCount, unit, minValue, maxValue, precisionFactor, pollIntervalMs, isEnabled)
VALUES 
(1, 'SEN-T01', N'1号棚温度传感器',  2, 1, 0,  1, N'℃',  -20, 80,     0.1, 2000, 1),
(2, 'SEN-H01', N'1号棚湿度传感器',  2, 2, 1,  1, N'%',   0,   100,   0.1, 2000, 1),
(3, 'SEN-L01', N'1号棚光照传感器',  2, 3, 2,  1, N'Lux', 0,   120000, 1,   5000, 1),
(4, 'SEN-SM01',N'1号棚土壤湿度',    2, 4, 3,  1, N'%',   0,   100,   0.1, 3000, 1),
(5, 'SEN-CO01',N'1号棚CO2传感器',   2, 5, 4,  1, N'ppm', 0,   5000,  1,   5000, 1),
(6, 'SEN-ST01',N'1号棚土壤温度',    2, 6, 5,  1, N'℃',  -10, 60,    0.1, 3000, 1),
(7, 'SEN-T02', N'2号棚温度传感器',  3, 1, 0,  1, N'℃',  -20, 80,    0.1, 2000, 1),
(8, 'SEN-H02', N'2号棚湿度传感器',  3, 2, 1,  1, N'%',   0,   100,   0.1, 2000, 1);
SET IDENTITY_INSERT Sensors OFF;
GO

-- ========== 5. 作物信息表（10条，涵盖常见温室作物） ==========
SET IDENTITY_INSERT CropInfo ON;
INSERT INTO CropInfo (id, cropName, variety, tempMin, tempMax, humidityMin, humidityMax, lightMin, lightMax, growthCycleDays, description)
VALUES 
(1,  N'番茄', N'大红番茄',   18, 32, 60, 85, 20000, 70000, 120, N'喜温作物，适宜昼夜温差10℃左右，充足光照可促进果实着色'),
(2,  N'番茄', N'樱桃番茄',   20, 30, 65, 80, 25000, 65000, 100, N'小型番茄品种，甜度高，适合鲜食，需搭架栽培'),
(3,  N'草莓', N'红颜草莓',   15, 25, 50, 70, 15000, 50000, 90,  N'日系品种，果大味甜，冬季大棚种植为主，需蜜蜂授粉'),
(4,  N'草莓', N'章姬草莓',   15, 25, 55, 75, 15000, 50000, 85,  N'长锥形草莓，口感细腻，适合采摘园种植'),
(5,  N'黄瓜', N'密刺黄瓜',   22, 32, 70, 90, 25000, 60000, 70,  N'喜温喜湿，需充足水分和光照，适时整枝打杈'),
(6,  N'辣椒', N'尖椒',       20, 30, 60, 80, 20000, 55000, 100, N'喜温不耐寒，开花期忌高温高湿，注意通风'),
(7,  N'生菜', N'结球生菜',   10, 22, 60, 80, 10000, 35000, 60,  N'喜凉作物，不耐高温，夏季需遮阳降温'),
(8,  N'葡萄', N'夏黑葡萄',   18, 30, 55, 75, 30000, 80000, 150, N'早熟无核品种，需搭架栽培，果实膨大期需控水'),
(9,  N'西瓜', N'8424西瓜',   25, 35, 50, 70, 35000, 90000, 90,  N'早中熟品种，喜光喜温，膨瓜期需大水大肥'),
(10, N'蓝莓', N'兔眼蓝莓',   15, 28, 50, 70, 20000, 60000, 180, N'需酸性土壤(pH4.5-5.5)，喜光耐旱，适合盆栽或地栽');
SET IDENTITY_INSERT CropInfo OFF;
GO

-- ========== 6. 大棚作物关联表（2条） ==========
-- growthStage: 1=苗期, 2=生长期, 3=开花期, 4=结果期, 5=收获期
SET IDENTITY_INSERT GreenhouseCrops ON;
INSERT INTO GreenhouseCrops (id, greenhouseId, cropId, plantingArea, plantingDate, expectedHarvestDate, growthStage, status, managerId, remark)
VALUES 
(1, 1, 1, 1000, '2025-02-15', '2025-06-15', 4, 1, 2, N'1号棚大红番茄，当前结果期'),
(2, 2, 3,  700, '2025-01-10', '2025-04-10', 5, 1, 2, N'2号棚红颜草莓，正在采摘');
SET IDENTITY_INSERT GreenhouseCrops OFF;
GO

-- ========== 7. 预警规则表（6条） ==========
-- alertLevel: 1=提示, 2=警告, 3=严重
SET IDENTITY_INSERT AlertRules ON;
INSERT INTO AlertRules (id, ruleName, sensorType, greenhouseId, minValue, maxValue, alertLevel, isEnabled, remark)
VALUES 
(1, N'温度过高预警',     1, 1, NULL, 35,   3, 1, N'1号棚温度超过35℃为严重预警'),
(2, N'温度过低预警',     1, 1, 5,   NULL, 3, 1, N'1号棚温度低于5℃为严重预警'),
(3, N'湿度过高预警',     2, 1, NULL, 90,   2, 1, N'1号棚湿度超过90%为警告'),
(4, N'CO2浓度过高预警',  5, 1, NULL, 2000, 2, 1, N'1号棚CO2超过2000ppm需通风'),
(5, N'土壤湿度过低预警', 4, NULL, 20, NULL, 2, 1, N'全局规则：土壤湿度低于20%需灌溉'),
(6, N'光照不足预警',     3, NULL, NULL, 5000, 1, 1, N'全局规则：光照低于5000Lux为提示');
SET IDENTITY_INSERT AlertRules OFF;
GO

-- ========== 8. 传感器采集数据表（20条模拟历史数据） ==========
SET IDENTITY_INSERT SensorData ON;
INSERT INTO SensorData (id, sensorId, deviceId, greenhouseId, temperature, humidity, lightIntensity, soilMoisture, co2Concentration, soilTemperature, timestamp, isAlert)
VALUES 
(1,  1, 2, 1, 24.5, 72.3, 35000, 45.2, 620,  19.8, DATEADD(HOUR, -20, GETDATE()), 0),
(2,  1, 2, 1, 23.8, 73.1, 32800, 44.8, 635,  19.5, DATEADD(HOUR, -19, GETDATE()), 0),
(3,  1, 2, 1, 22.6, 74.5, 30000, 44.1, 650,  19.2, DATEADD(HOUR, -18, GETDATE()), 0),
(4,  1, 2, 1, 21.5, 76.2, 25000, 43.5, 680,  18.9, DATEADD(HOUR, -17, GETDATE()), 0),
(5,  1, 2, 1, 20.3, 78.0, 18000, 42.9, 710,  18.5, DATEADD(HOUR, -16, GETDATE()), 0),
(6,  1, 2, 1, 19.8, 79.5, 12000, 42.3, 740,  18.2, DATEADD(HOUR, -15, GETDATE()), 0),
(7,  1, 2, 1, 19.2, 80.8, 5000,  41.8, 780,  17.9, DATEADD(HOUR, -14, GETDATE()), 0),
(8,  1, 2, 1, 18.8, 82.1, 2000,  41.2, 820,  17.6, DATEADD(HOUR, -13, GETDATE()), 0),
(9,  1, 2, 1, 18.5, 83.0, 800,   41.7, 860,  17.3, DATEADD(HOUR, -12, GETDATE()), 0),
(10, 1, 2, 1, 19.0, 81.5, 1500,  40.1, 830,  17.5, DATEADD(HOUR, -11, GETDATE()), 0),
(11, 1, 2, 1, 20.5, 78.2, 8000,  39.5, 750,  18.0, DATEADD(HOUR, -10, GETDATE()), 0),
(12, 1, 2, 1, 22.3, 75.0, 22000, 38.8, 680,  18.5, DATEADD(HOUR, -9, GETDATE()),  0),
(13, 1, 2, 1, 25.1, 70.5, 38000, 37.2, 620,  19.2, DATEADD(HOUR, -8, GETDATE()),  0),
(14, 1, 2, 1, 28.4, 66.8, 52000, 36.5, 580,  20.0, DATEADD(HOUR, -7, GETDATE()),  0),
(15, 1, 2, 1, 31.2, 63.5, 60000, 35.8, 550,  20.8, DATEADD(HOUR, -6, GETDATE()),  0),
(16, 1, 2, 1, 33.5, 61.0, 65000, 35.2, 520,  21.5, DATEADD(HOUR, -5, GETDATE()),  1),
(17, 1, 2, 1, 36.5, 58.2, 68000, 34.5, 490,  22.3, DATEADD(HOUR, -4, GETDATE()),  1),
(18, 1, 2, 1, 34.2, 60.5, 62000, 34.0, 510,  21.8, DATEADD(HOUR, -3, GETDATE()),  0),
(19, 1, 2, 1, 30.8, 64.0, 48000, 33.5, 550,  21.0, DATEADD(HOUR, -2, GETDATE()),  0),
(20, 1, 2, 1, 27.5, 68.5, 35000, 33.0, 590,  20.2, DATEADD(HOUR, -1, GETDATE()), 0);
SET IDENTITY_INSERT SensorData OFF;
GO

-- ========== 9. 预警记录表（2条示例） ==========
-- handleStatus: 0=未处理, 1=已处理
SET IDENTITY_INSERT AlertRecords ON;
INSERT INTO AlertRecords (id, ruleId, sensorDataId, greenhouseId, deviceId, sensorId, alertType, alertLevel, triggerValue, thresholdValue, message, handleStatus, handlerId, handleTime, handleRemark, createdAt)
VALUES 
(1, 1, NULL, 1, 2, 1, 1, 3, 36.5, 35, N'1号棚温度达到36.5℃，超过阈值35℃', 1, 1, GETDATE(), N'已开启通风系统降温', DATEADD(HOUR, -2, GETDATE())),
(2, 3, NULL, 1, 2, 2, 2, 2, 92.0, 90, N'1号棚湿度达到92%，超过阈值90%', 0, NULL, NULL, NULL, DATEADD(MINUTE, -30, GETDATE()));
SET IDENTITY_INSERT AlertRecords OFF;
GO

-- ========== 10. 控制指令表（3条示例） ==========
-- commandType: 1=通风, 2=灌溉, 3=补光, 4=遮阳, 5=加温
-- status: 0=待执行, 1=执行中, 2=已完成, 3=执行失败
-- sendType: 1=手动, 2=自动
SET IDENTITY_INSERT ControlCommands ON;
INSERT INTO ControlCommands (id, deviceId, greenhouseId, commandType, commandParam, status, userId, sendType, resultMessage, sendTime, executeTime, remark)
VALUES 
(1, 4, 1, 1, N'开启3档',    2, 1, 1, N'通风系统已启动，3档运行',    DATEADD(HOUR, -2, GETDATE()), DATEADD(HOUR, -2, GETDATE()), N'手动通风降温'),
(2, 4, 1, 2, N'灌溉15分钟', 2, 2, 2, N'灌溉完成，用水量2.3m³',     DATEADD(DAY, -1, GETDATE()),  DATEADD(DAY, -1, GETDATE()),  N'土壤湿度低自动灌溉'),
(3, 4, 1, 3, N'补光2小时',  1, 1, 2, NULL,                          GETDATE(),                    NULL,                         N'光照不足自动补光执行中');
SET IDENTITY_INSERT ControlCommands OFF;
GO

-- ========== 11. 系统配置表（8条） ==========
SET IDENTITY_INSERT SystemConfig ON;
INSERT INTO SystemConfig (id, configKey, configValue, configGroup, description, isEnabled)
VALUES 
(1, 'data_retention_days',    '90',                  N'数据管理', N'传感器数据保留天数',       1),
(2, 'collect_interval_ms',    '2000',                N'采集设置', N'默认采集间隔(毫秒)',       1),
(3, 'alert_enabled',          'true',                N'预警设置', N'是否启用预警功能',         1),
(4, 'alert_check_interval',   '30',                  N'预警设置', N'预警检查间隔(秒)',          1),
(5, 'virtual_mode_port',      '5020',                N'虚拟模式', N'虚拟Modbus从站端口',       1),
(6, 'auto_irrigation',        'true',                N'自动控制', N'是否启用自动灌溉',         1),
(7, 'soil_moisture_threshold','30',                  N'自动控制', N'自动灌溉土壤湿度阈值(%)', 1),
(8, 'system_name',            N'智慧农业监控系统',   N'系统信息', N'系统名称',                 1);
SET IDENTITY_INSERT SystemConfig OFF;
GO

-- ========== 12. 登录记录表（3条示例） ==========
INSERT INTO LoginRecords (username, userId, loginTime, isSuccess, failReason, ipAddress, deviceInfo)
VALUES 
('admin',    1, DATEADD(DAY, -3, GETDATE()), 1, NULL, N'127.0.0.1',    N'Windows PC'),
('admin',    1, DATEADD(DAY, -1, GETDATE()), 1, NULL, N'127.0.0.1',    N'Windows PC'),
('farmer01', 2, GETDATE(),                    1, NULL, N'192.168.1.50', N'Windows PC');
GO

-- 现在添加外键约束
-- 添加Users表的外键约束
ALTER TABLE Greenhouses ADD CONSTRAINT FK_Greenhouses_Users_ManagerId FOREIGN KEY (managerId) REFERENCES Users(id);
ALTER TABLE GreenhouseCrops ADD CONSTRAINT FK_GreenhouseCrops_Users_ManagerId FOREIGN KEY (managerId) REFERENCES Users(id);
ALTER TABLE AlertRecords ADD CONSTRAINT FK_AlertRecords_Users_HandlerId FOREIGN KEY (handlerId) REFERENCES Users(id);
ALTER TABLE ControlCommands ADD CONSTRAINT FK_ControlCommands_Users_UserId FOREIGN KEY (userId) REFERENCES Users(id);
ALTER TABLE LoginRecords ADD CONSTRAINT FK_LoginRecords_Users_UserId FOREIGN KEY (userId) REFERENCES Users(id);

-- 添加Greenhouses表的外键约束
ALTER TABLE Devices ADD CONSTRAINT FK_Devices_Greenhouses_GreenhouseId FOREIGN KEY (greenhouseId) REFERENCES Greenhouses(id);
ALTER TABLE AlertRules ADD CONSTRAINT FK_AlertRules_Greenhouses_GreenhouseId FOREIGN KEY (greenhouseId) REFERENCES Greenhouses(id);
ALTER TABLE SensorData ADD CONSTRAINT FK_SensorData_Greenhouses_GreenhouseId FOREIGN KEY (greenhouseId) REFERENCES Greenhouses(id);
ALTER TABLE AlertRecords ADD CONSTRAINT FK_AlertRecords_Greenhouses_GreenhouseId FOREIGN KEY (greenhouseId) REFERENCES Greenhouses(id);
ALTER TABLE ControlCommands ADD CONSTRAINT FK_ControlCommands_Greenhouses_GreenhouseId FOREIGN KEY (greenhouseId) REFERENCES Greenhouses(id);
ALTER TABLE GreenhouseCrops ADD CONSTRAINT FK_GreenhouseCrops_Greenhouses_Id FOREIGN KEY (greenhouseId) REFERENCES Greenhouses(id);

-- 添加Devices表的外键约束
ALTER TABLE Sensors ADD CONSTRAINT FK_Sensors_Devices_DeviceId FOREIGN KEY (deviceId) REFERENCES Devices(id);
ALTER TABLE SensorData ADD CONSTRAINT FK_SensorData_Devices_DeviceId FOREIGN KEY (deviceId) REFERENCES Devices(id);
ALTER TABLE AlertRecords ADD CONSTRAINT FK_AlertRecords_Devices_DeviceId FOREIGN KEY (deviceId) REFERENCES Devices(id);
ALTER TABLE ControlCommands ADD CONSTRAINT FK_ControlCommands_Devices_DeviceId FOREIGN KEY (deviceId) REFERENCES Devices(id);

-- 添加Sensors表的外键约束
ALTER TABLE SensorData ADD CONSTRAINT FK_SensorData_Sensors_SensorId FOREIGN KEY (sensorId) REFERENCES Sensors(id);
ALTER TABLE AlertRecords ADD CONSTRAINT FK_AlertRecords_Sensors_SensorId FOREIGN KEY (sensorId) REFERENCES Sensors(id);

-- 添加CropInfo表的外键约束
ALTER TABLE GreenhouseCrops ADD CONSTRAINT FK_GreenhouseCrops_CropInfo_CropId FOREIGN KEY (cropId) REFERENCES CropInfo(id);

-- 添加AlertRules表的外键约束
ALTER TABLE AlertRecords ADD CONSTRAINT FK_AlertRecords_AlertRules_RuleId FOREIGN KEY (ruleId) REFERENCES AlertRules(id);

-- 添加SensorData表的外键约束
ALTER TABLE AlertRecords ADD CONSTRAINT FK_AlertRecords_SensorData_SensorDataId FOREIGN KEY (sensorDataId) REFERENCES SensorData(id);

-- 重置自增种子，确保后续插入的ID从已有最大值之后继续
DBCC CHECKIDENT ('Users', RESEED);
DBCC CHECKIDENT ('Greenhouses', RESEED);
DBCC CHECKIDENT ('Devices', RESEED);
DBCC CHECKIDENT ('Sensors', RESEED);
DBCC CHECKIDENT ('CropInfo', RESEED);
DBCC CHECKIDENT ('GreenhouseCrops', RESEED);
DBCC CHECKIDENT ('AlertRules', RESEED);
DBCC CHECKIDENT ('SensorData', RESEED);
DBCC CHECKIDENT ('AlertRecords', RESEED);
DBCC CHECKIDENT ('ControlCommands', RESEED);
DBCC CHECKIDENT ('SystemConfig', RESEED);
GO

PRINT N'数据库 SmartAgricultureDB 初始化完成！共创建 13 张表，已插入种子数据。';
PRINT N'默认账号：admin / Admin@123456（管理员）、farmer01 / Admin@123456（农户）';
GO