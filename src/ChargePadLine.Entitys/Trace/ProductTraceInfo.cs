using ChargePadLine.Entitys.Systems;
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

namespace ChargePadLine.Entitys.Trace
{
    /// <summary>
    /// 产品信息追溯
    /// </summary>
    [Table("mes_productTraceInfo")]
    public class ProductTraceInfo
    {
        /// <summary>
        /// 产品追踪信息ID
        /// </summary>
        [Key]
        public Guid ProductTraceId { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string Sfc { get; set; }

        /// <summary>
        /// 站点(工厂或生产站点的标识)
        /// </summary>
        public string Site​​ { get; set; }

        /// <summary>
        /// 活动ID（唯一标识一个具体的生产活动、工单或任务步骤 ）       
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// 是否OK
        /// </summary>
        public bool IsOK { get; set; }

        /// <summary>
        /// 资源(执行操作所涉及的具体资源，如设备编号、产线编号、工位编号)
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// 数据采集组版本号
        /// </summary>
        public string DcGroupRevision { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTimeOffset SendTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 产品参数组
        /// </summary>
        public List<ParameterData>? parametricDataArray { get; set; } = new List<ParameterData>();
    }

    public class ProductTraceInfoClaimEntityConfiguration : IEntityTypeConfiguration<ProductTraceInfo>
    {
        public void Configure(EntityTypeBuilder<ProductTraceInfo> builder)
        {
            builder.HasKey(e => e.ProductTraceId);

            builder.Property(e => e.parametricDataArray)
                  .HasConversion(
                      v => v == null ? "[]" : JsonSerializer.Serialize<List<ParameterData>>(v, new JsonSerializerOptions()),
                      v => string.IsNullOrEmpty(v)
                           ? new List<ParameterData>()
                           : JsonSerializer.Deserialize<List<ParameterData>>(v, new JsonSerializerOptions())
                             ?? new List<ParameterData>()
                  );
        }
    }
}
