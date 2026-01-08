using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.InventoryManagement;
using ChargePadLine.Entitys.Trace.Order;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.Product;
using ChargePadLine.Entitys.Trace.Production.BatchQueue;
using ChargePadLine.Entitys.Trace.QualityManagement;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Entitys.Trace.BOM;
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

      #region System配置
      modelBuilder.ApplyConfiguration(new SysUserClaimEntityConfiguration());
      modelBuilder.ApplyConfiguration(new SysUserRoleClaimEntityConfiguration());
      modelBuilder.ApplyConfiguration(new SysMenuClaimEntityConfiguration());
      modelBuilder.ApplyConfiguration(new SysRoleClaimEntityConfiguration());
      modelBuilder.ApplyConfiguration(new SysRoleMenuClaimEntityConfiguration());
      modelBuilder.ApplyConfiguration(new SysDeptClaimEntityConfiguration());
      modelBuilder.ApplyConfiguration(new SysRoleDeptClaimEntityConfiguration());
      modelBuilder.ApplyConfiguration(new SysOperationLogClaimEntityConfiguration());
      #endregion

      #region 业务模块
      modelBuilder.ApplyConfiguration(new MaterialEntityTypeConfiguration());
      modelBuilder.ApplyConfiguration(new BomRecipeEntityTypeConfiguration());
      modelBuilder.ApplyConfiguration(new WorkOrderEntityTypeConfiguration());
      modelBuilder.ApplyConfiguration(new TraceInfoEntityTypeConfiguration());
      modelBuilder.ApplyConfiguration(new TraceProcItemEntityTypeConfiguration());
      modelBuilder.ApplyConfiguration(new MaterialBatchQueueItemEntityTypeConfiguration());
      modelBuilder.ApplyConfiguration(new ProductTraceInfoClaimEntityConfiguration());
      #endregion


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
    public DbSet<SysOperationLog> SysOperationLogs { get; set; }
    #endregion

    #region 业务模块
    //产站设备
    public DbSet<ProductionLine> ProductionLines { get; set; }
    public DbSet<Deviceinfo> DeviceInfos { get; set; }
    public DbSet<ProductTraceInfo> ProductTraceInfos { get; set; }

    //物料bom
    public DbSet<Material> Materials { get; set; }
    public DbSet<BomRecipe> BomRecipes { get; set; }
    public DbSet<BomRecipeItem> BomRecipeItems { get; set; }

    //工单
    // public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderExecution> WorkOrderExecutions { get; set; }
    public DbSet<OrderList> OrderList { get; set; }

    //追溯信息
    public DbSet<TraceInfo> TraceInfos { get; set; }
    public DbSet<TraceBomItem> TraceBomItems { get; set; }
    public DbSet<TraceProcItem> TraceProcItems { get; set; }

    //批次排队
    public DbSet<BatchMaterialQueueItem> BatchMaterialQueueItems { get; set; }
    public DbSet<CtrlVsn> CtrlVs { get; set; }

    //工艺路线
    public DbSet<StationList> StationList { get; set; }
    public DbSet<ProcessRoute> ProcessRoutes { get; set; }
    public DbSet<ProcessRouteItem> ProcessRouteItems { get; set; }
    public DbSet<RoutingOperation> RoutingOperations { get; set; }
    public DbSet<ProductProcessState> ProductProcessStates { get; set; }

    //质量检验
    public DbSet<DefectRecord> DefectRecords { get; set; }
    public DbSet<QualityInspectionDetail> QualityInspectionDetails { get; set; }
    public DbSet<QualityInspectionItem> QualityInspectionItems { get; set; }
    public DbSet<QualityInspectionRecord> QualityInspectionRecords { get; set; }
    public DbSet<QualityInspectionStandard> QualityInspectionStandards { get; set; }

    //产品列表
    public DbSet<ProductList> ProductList { get; set; }

    //BOM管理
    public DbSet<BomList> BomList { get; set; }
    public DbSet<BomItem> BomItem { get; set; }
    #endregion

    #region 库存管理
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<InventoryMaterial> InventoryMaterials { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<InventoryStocktake> inventoryStocktakes { get; set; }


    #endregion
  }
}