using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace;

namespace ChargePadLine.DbContexts
{
    public class AppDbContext : DbContext
    {
        private readonly IHostEnvironment _env;
        public AppDbContext(DbContextOptions<AppDbContext> options, IHostEnvironment env) : base(options)
        {
            this._env = env;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new SysUserClaimEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SysMenuClaimEntityConfiguration());
 

            // 配置SysDept实体的主键自增
            modelBuilder.Entity<SysDept>(entity =>
            {
                entity.HasKey(e => e.DeptId);
                // 使用ValueGeneratedOnAdd让EF Core自动处理主键生成
                entity.Property(e => e.DeptId).ValueGeneratedOnAdd();
            });

            // 配置SysRole实体的主键自增
            modelBuilder.Entity<SysRole>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                // 使用ValueGeneratedOnAdd让EF Core自动处理主键生成
                entity.Property(e => e.RoleId).ValueGeneratedOnAdd();
            });

            // 配置SysOperationLog实体，确保表名映射正确
            // 注意：表名已在实体类的 [Table("sys_operation_log")] 特性中定义，无需再次设置
            modelBuilder.Entity<SysOperationLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.LogId).ValueGeneratedOnAdd();

                // 配置 OperationTime 字段：将 DateTimeOffset 转换为 DateTime 以兼容 PostgreSQL
                entity.Property(e => e.OperationTime)
                    .HasConversion(
                        v => v.DateTime,  // 存储时转换为 DateTime
                        v => new DateTimeOffset(v));  // 读取时转换回 DateTimeOffset
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

        #region System配置
        public DbSet<SysUser> SysUsers { get; set; }
        public DbSet<SysRole> SysRoles { get; set; }
        public DbSet<SysMenu> SysMenus { get; set; }
        public DbSet<SysDept> SysDepts { get; set; }
        public DbSet<SysPost> SysPosts { get; set; }
        public DbSet<SysUserRole> SysUserRole { get; set; }
        public DbSet<SysRoleDept> SysRoleDept { get; set; }
        public DbSet<SysRoleMenu> SysRoleMenu { get; set; }
        public DbSet<SysUserPost> SysUserPost { get; set; }
     
        #endregion

        #region 业务模块
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<DeviceInfo> DeviceInfos { get; set; }
     
        public DbSet<ProductTraceInfo> ProductTraceInfos { get; set; }
        #endregion

        #region 系统日志
        public DbSet<SysOperationLog> SysOperationLogs { get; set; }
        #endregion
    }
}