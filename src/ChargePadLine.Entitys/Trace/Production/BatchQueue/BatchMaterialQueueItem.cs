using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Production.BatchQueue
{
    /// <summary>
    /// 物料批生产队列
    /// </summary>
    [Table("mes_prod_material_batch_queue")]
    public class BatchMaterialQueueItem
    {
        public int Id { get; set; }

        /// <summary>
        /// BOM项编码
        /// </summary>
        public string BomItemCode { get; set; } = "";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string BatchCode { get; set; } = String.Empty;

        /// <summary>
        /// 上料数
        /// </summary>
        public int TotalAmount { get; set; }

        /// <summary>
        /// 剩余数量
        /// </summary>
        public int RemainingAmount { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTimeOffset DeletedAt { get; set; }

    }
    public class MaterialBatchQueueItemEntityTypeConfiguration : IEntityTypeConfiguration<BatchMaterialQueueItem>
    {
        public void Configure(EntityTypeBuilder<BatchMaterialQueueItem> builder)
        {
            builder.HasIndex(x => new { x.BomItemCode, x.BatchCode }).IsUnique();    // 重复性校验
            builder.HasIndex(x => new { x.IsDeleted, x.BomItemCode, x.Priority, x.CreatedAt, });
        }
    }
}
