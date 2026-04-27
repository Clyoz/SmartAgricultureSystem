using System;

namespace SmartAgricultureSystem.Models
{
    /// <summary>
    /// 大棚作物关联模型
    /// 记录大棚中种植的作物信息（大棚与作物的多对多关系）
    /// </summary>
    public class GreenhouseCrop
    {
        /// <summary>主键，自增ID</summary>
        public int id { get; set; }

        /// <summary>大棚ID（关联Greenhouses表）</summary>
        public int greenhouseId { get; set; }

        /// <summary>作物ID（关联CropInfo表）</summary>
        public int cropId { get; set; }

        /// <summary>种植面积（平方米）</summary>
        public double plantingArea { get; set; }

        /// <summary>种植日期</summary>
        public DateTime plantingDate { get; set; }

        /// <summary>预计收获日期</summary>
        public DateTime? expectedHarvestDate { get; set; }

        /// <summary>实际收获日期</summary>
        public DateTime? actualHarvestDate { get; set; }

        /// <summary>生长阶段：1=育苗, 2=生长期, 3=开花期, 4=结果期, 5=收获期</summary>
        public int growthStage { get; set; } = 1;

        /// <summary>种植状态：1=种植中, 2=已收获, 3=已清除</summary>
        public int status { get; set; } = 1;

        /// <summary>负责人ID</summary>
        public int? managerId { get; set; }

        /// <summary>备注</summary>
        public string remark { get; set; }

        /// <summary>创建时间</summary>
        public DateTime createdAt { get; set; } = DateTime.Now;

        /// <summary>更新时间</summary>
        public DateTime? updatedAt { get; set; }
    }
}
