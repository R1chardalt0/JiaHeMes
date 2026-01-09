using ChargePadLine.Entitys.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChargePadLine.Entitys.Trace.TraceInformation
{
    /// <summary>
    /// SN历史记录表
    /// </summary>
    [Table("mes_sn_list_history")]
    public class MesSnListHistory : BaseEntity
    {
        /// <summary>
        /// 状态ID
        /// </summary>
        [Description("状态ID")]
        [Key]
        public Guid SNListHistoryId { get; set; }

        /// <summary>
        /// 序列号/产品唯一码
        /// </summary>
        [Description("序列号/产品唯一码")]
        public string SnNumber { get; set; } = "";

        /// <summary>
        /// 产品ID
        /// </summary>
        [Description("产品ID")]
        public Guid ProductListId { get; set; }

        /// <summary>
        /// 工单ID
        /// </summary>
        [Description("工单ID")]
        public Guid OrderListId { get; set; }

        /// <summary>
        /// 当前状态：1-合格，2-不合格，3-已包装，4-已入库，5-跳站
        /// </summary>
        [Description("当前状态：1-合格，2-不合格，3-已包装，4-已入库，5-跳站")]
        public int StationStatus { get; set; }

        /// <summary>
        /// 当前站点ID
        /// </summary>
        [Description("当前站点ID")]
        public Guid CurrentStationListId { get; set; }

        /// <summary>
        /// 线体ID
        /// </summary>
        [Description("线体ID")]
        public Guid ProductionLineId { get; set; }

        /// <summary>
        /// 当前设备ID
        /// </summary>
        [Description("当前设备ID")]
        public Guid ResourceId { get; set; }

        /// <summary>
        /// 测试数据
        /// </summary>
        [Description("测试数据")]
        public string TestResults { get; set; } = "";

        /// <summary>
        /// 批次数据
        /// </summary>
        [Description("批次数据")]
        public string BatchResults { get; set; } = "";

        /// <summary>
        /// 是否异常
        /// </summary>
        [Description("是否异常")]
        public bool IsAbnormal { get; set; }

        /// <summary>
        /// 异常代码
        /// </summary>
        [Description("异常代码")]
        public string AbnormalCode { get; set; } = "";

        /// <summary>
        /// 异常描述
        /// </summary>
        [Description("异常描述")]
        public string AbnormalDescription { get; set; } = "";

        /// <summary>
        /// 是否锁定（异常锁定）
        /// </summary>
        [Description("是否锁定（异常锁定）")]
        public bool IsLocked { get; set; }

        /// <summary>
        /// 返工次数
        /// </summary>
        [Description("返工次数")]
        public int ReworkCount { get; set; }

        /// <summary>
        /// 是否正在返工
        /// </summary>
        [Description("是否正在返工")]
        public bool IsReworking { get; set; }

        /// <summary>
        /// 返工原因
        /// </summary>
        [Description("返工原因")]
        public string ReworkReason { get; set; } = "";

        /// <summary>
        /// 返工时间
        /// </summary>
        [Description("返工时间")]
        public DateTimeOffset? ReworkTime { get; set; }
    }

    public class MesSnListHistoryEntityTypeConfiguration : IEntityTypeConfiguration<MesSnListHistory>
    {
        public void Configure(EntityTypeBuilder<MesSnListHistory> builder)
        {
            // 主键配置
            builder.HasKey(e => e.SNListHistoryId);

            // 常用索引
            builder.HasIndex(e => e.SnNumber);
            builder.HasIndex(e => e.ProductListId);
            builder.HasIndex(e => e.OrderListId);
            builder.HasIndex(e => e.StationStatus);
            builder.HasIndex(e => e.CreateTime);
        }
    }
}