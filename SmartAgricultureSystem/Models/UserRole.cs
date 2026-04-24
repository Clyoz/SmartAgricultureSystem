namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 用户角色枚举
    /// Admin    - 管理员：可管理设备、查看所有数据
    /// Farmer   - 普通农户：只能查看自己大棚数据、处理预警
    /// </summary>
    public enum UserRole
    {
        /// <summary>管理员</summary>
        Admin = 0,
        /// <summary>普通农户</summary>
        Farmer = 1
    }
}