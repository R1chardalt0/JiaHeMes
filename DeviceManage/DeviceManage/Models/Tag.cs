using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceManage.Models
{
    /// <summary>
    /// 数据点位实体
    /// </summary>
    [Table("dm_tag")]
    public class Tag
    {
        /// <summary>
        /// 点位实体ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// plc设备ID
        /// </summary>
        [Required]
        public int PlcDeviceId { get; set; }

        /// <summary>
        /// plc设备导航属性
        /// </summary>
        [ForeignKey(nameof(PlcDeviceId))]
        public PlcDevice PlcDevice { get; set; } = null!;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 被哪些配方项引用（可选，用于反向查询）
        /// </summary>
        public ICollection<RecipeItem>? RecipeItems { get; set; } = new List<RecipeItem>();

        /// <summary>
        /// 点位映射实体
        /// </summary>
        public List<TagDetail>? TagDetailDataArray { get; set; } = new List<TagDetail>();
    }

    public class TagClaimEntityConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.TagDetailDataArray)
                  .HasConversion(
                      v => v == null ? "[]" : JsonSerializer.Serialize<List<TagDetail>>(v, new JsonSerializerOptions()),
                      v => string.IsNullOrEmpty(v)
                           ? new List<TagDetail>()
                           : JsonSerializer.Deserialize<List<TagDetail>>(v, new JsonSerializerOptions())
                             ?? new List<TagDetail>()
                  );
        }
    }

    public class TagDetail
    {
        /// <summary>
        /// 点位名称
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 数据类型
        /// </summary>
        [Required]
        public DataType DataType { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [MaxLength(20)]
        public string? Unit { get; set; } // 如 "RPM", "°C"
        /// <summary>
        /// 描述信息
        /// </summary>
        [MaxLength(200)]
        public string Remarks { get; set; } = string.Empty;
    }

    public enum DataType
    {
        Bool,
        Char,
        Byte,
        SByte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,
        String60
    }
}
