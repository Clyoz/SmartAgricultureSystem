using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 大棚模型
    /// 代表一个温室大棚，包含地理位置、面积等信息
    /// </summary>
    public class Greenhouse
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>大棚编号（唯一，如GH-001）</summary>
        public string greenhouseCode { get; set; }

        /// <summary>大棚名称</summary>
        public string name { get; set; }

        /// <summary>大棚位置</summary>
        public string location { get; set; }

        /// <summary>面积（平方米）</summary>
        public double area { get; set; }

        /// <summary>大棚类型（玻璃温室、塑料大棚等）</summary>
        public string greenhouseType { get; set; }

        /// <summary>负责人ID（关联Users表）</summary>
        public int? managerId { get; set; }

        /// <summary>建设日期</summary>
        public DateTime? buildDate { get; set; }

        /// <summary>当前状态：1=空闲, 2=种植中, 3=维护中</summary>
        public int status { get; set; } = 1;

        /// <summary>备注</summary>
        public string remark { get; set; }

        /// <summary>创建时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;

        /// <summary>更新时间</summary>
        public DateTime? updatedAt { get; set; }
    }
}
