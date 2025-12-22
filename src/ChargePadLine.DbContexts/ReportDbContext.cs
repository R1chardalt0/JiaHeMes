using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChargePadLine.DbContexts
{
    public class ReportDbContext : DbContext
    {
        private readonly IHostEnvironment _env;
        public ReportDbContext(DbContextOptions<ReportDbContext> options, IHostEnvironment env) : base(options)
        {
            this._env = env;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductTraceInfo>(entity =>
            {
                entity.HasKey(e => e.ProductTraceId);

                entity.Property(e => e.parametricDataArray)
                      .HasConversion(
                          v => v == null ? "[]" : JsonSerializer.Serialize<List<ParameterData>>(v, new JsonSerializerOptions()),
                          v => string.IsNullOrEmpty(v)
                               ? new List<ParameterData>()
                               : JsonSerializer.Deserialize<List<ParameterData>>(v, new JsonSerializerOptions())
                                 ?? new List<ParameterData>()
                      );
            });
        }

        #region 业务模块
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }
        public DbSet<ProductTraceInfo> ProductTraceInfos { get; set; }
        #endregion
    }
}
