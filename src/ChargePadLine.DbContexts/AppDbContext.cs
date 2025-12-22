using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.Production.BatchQueue;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInfo;
using ChargePadLine.Entitys.Trace.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text.Json;

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
            modelBuilder.ApplyConfiguration(new SysUserRoleClaimEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SysMenuClaimEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SysRoleClaimEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SysRoleMenuClaimEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SysDeptClaimEntityConfiguration()); 
            modelBuilder.ApplyConfiguration(new SysRoleDeptClaimEntityConfiguration());

            modelBuilder.ApplyConfiguration(new MaterialEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BomRecipeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TraceInfoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TraceProcItemEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialBatchQueueItemEntityTypeConfiguration());

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
                          v => v == null ? "[]" : JsonSerializer.Serialize<List<ParameterData>>(v, new JsonSerializerOptions()),
                          v => string.IsNullOrEmpty(v)
                               ? new List<ParameterData>()
                               : JsonSerializer.Deserialize<List<ParameterData>>(v, new JsonSerializerOptions())
                                 ?? new List<ParameterData>()
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

        public DbSet<Material> Materials { get; set; }
        public DbSet<BomRecipe> BomRecipes { get; set; }
        public DbSet<BomRecipeItem> BomRecipeItems { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<WorkOrderExecution> WorkOrderExecutions { get; set; }
        public DbSet<TraceInfo> TraceInfos { get; set; }
        public DbSet<TraceBomItem> TraceBomItems { get; set; }
        public DbSet<TraceProcItem> TraceProcItems { get; set; }
        public DbSet<BatchMaterialQueueItem> BatchMaterialQueueItems { get; set; }
        #endregion

        #region 系统日志
        public DbSet<SysOperationLog> SysOperationLogs { get; set; }
        #endregion
    }
}