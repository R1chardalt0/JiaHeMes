using ChargePadLine.Entitys.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace ChargePadLine.Entitys.Trace.TraceInformation
{
    /// <summary>
    /// SN测试数据表
    /// </summary>
    [Table("mes_sn_test_data")]
    public class MesSnTestData : BaseEntity
    {
        /// <summary>
        /// 测试数据ID
        /// </summary>
        [Description("测试数据ID")]
        [Key]
        public Guid SNTestDataId { get; set; }

        /// <summary>
        /// 测试数据主表ID
        /// </summary>
        [Description("测试数据主表ID")]
        public Guid SNListHistoryId { get; set; }

        /// <summary>
        /// 上限
        /// </summary>
        [Description("上限")]
        [Column(TypeName = "numeric(32,0)")]
        public decimal Upperlimit { get; set; }

        /// <summary>
        /// 下限
        /// </summary>
        [Description("下限")]
        [Column(TypeName = "numeric(32,0)")]
        public decimal Lowerlimit { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [Description("单位")]
        public string? Units { get; set; }

        /// <summary>
        /// 测试项目
        /// </summary>
        [Description("测试项目")]
        public string? ParametricKey { get; set; }

        /// <summary>
        /// 测试结果
        /// </summary>
        [Description("测试结果")]
        public string? TestResult { get; set; }

        /// <summary>
        /// 测试值
        /// </summary>
        [Description("测试值")]
        public string? TestValue { get; set; }

       
    }

    public class MesSnTestDataEntityTypeConfiguration : IEntityTypeConfiguration<MesSnTestData>
    {
        public void Configure(EntityTypeBuilder<MesSnTestData> builder)
        {
            // 主键配置
            builder.HasKey(e => e.SNTestDataId);

            // 外键配置
            builder.HasOne<MesSnListHistory>()
                .WithMany()
                .HasForeignKey(e => e.SNListHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // 索引配置
            builder.HasIndex(e => e.SNListHistoryId);
            builder.HasIndex(e => e.ParametricKey);
            builder.HasIndex(e => e.TestResult);
            builder.HasIndex(e => e.SearchValue);
        }
    }
}