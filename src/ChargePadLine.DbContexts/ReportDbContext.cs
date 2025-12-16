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

            // 配置SysCompany实体
            modelBuilder.Entity<SysCompany>(entity =>
            {
                entity.HasKey(e => e.CompanyId);
                entity.Property(e => e.CompanyId).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.CompanyCode).IsUnique();
                entity.HasIndex(e => e.CompanyName).IsUnique();
            });

            modelBuilder.Entity<EquipmentTracinfo>(entity =>
            {
                entity.HasKey(e => e.EquipmentTraceId);

                entity.Property(e => e.Parameters)
                      .HasConversion(
                          v => v == null ? "[]" : JsonSerializer.Serialize<List<Iotdata>>(v, new JsonSerializerOptions()),
                          v => string.IsNullOrEmpty(v)
                               ? new List<Iotdata>()
                               : JsonSerializer.Deserialize<List<Iotdata>>(v, new JsonSerializerOptions())
                                 ?? new List<Iotdata>()
                      );
            });

            modelBuilder.Entity<ProductTraceInfo>(entity =>
            {
                entity.HasKey(e => e.ProductTraceId);

                entity.Property(e => e.parametricDataArray)
                      .HasConversion(
                          v => v == null ? "[]" : JsonSerializer.Serialize<List<Iotdata>>(v, new JsonSerializerOptions()),
                          v => string.IsNullOrEmpty(v)
                               ? new List<Iotdata>()
                               : JsonSerializer.Deserialize<List<Iotdata>>(v, new JsonSerializerOptions())
                                 ?? new List<Iotdata>()
                      );
            });
        }

        #region 业务模块
        public DbSet<SysCompany> SysCompanys { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }
        public DbSet<EquipmentTracinfo> EquipmentTracinfos { get; set; }
        public DbSet<ProductTraceInfo> ProductTraceInfos { get; set; }
        #endregion
    }
}
