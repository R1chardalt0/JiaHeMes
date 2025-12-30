using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ChargePadLine.Service.Trace.Impl;

namespace ChargePadLine.Tests
{
  public class WorkOrderServiceTest
  {
    private Mock<ILogger<WorkOrderService>>? _loggerMock;
    private AppDbContext? _dbContext;
    private WorkOrderService? _workOrderService;

    // 为每个测试创建独立的数据库上下文
    private void InitializeTest()
    {
      _loggerMock = new Mock<ILogger<WorkOrderService>>();

      // 使用唯一数据库名称，确保每个测试使用独立的内存数据库
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
          .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
          .Options;

      var hostEnvMock = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
      hostEnvMock.Setup(x => x.EnvironmentName).Returns("Development");

      _dbContext = new AppDbContext(options, hostEnvMock.Object);

      // 清除数据库，确保每次测试都是干净的
      _dbContext.Database.EnsureDeleted();
      _dbContext.Database.EnsureCreated();

      // 创建实际的Repository实例
      var workOrderRepo = new Repository<WorkOrder>(_dbContext);
      var bomRecipeRepo = new Repository<BomRecipe>(_dbContext);
      var materialRepo = new Repository<Material>(_dbContext);

      _workOrderService = new WorkOrderService(
          workOrderRepo,
          bomRecipeRepo,
          materialRepo,
          _loggerMock.Object
      );
    }

    // 释放数据库资源
    private void CleanupTest()
    {
      _dbContext?.Dispose();
    }

    [Fact]
    public async Task CreateWorkOrderAsync_WithValidData_ShouldReturnWorkOrderDto()
    {
      // 初始化测试环境
      InitializeTest();

      try
      {
        // Arrange
        // 先创建BOM配方
        var bomRecipe = new BomRecipe
        {
          BomCode = new BomCode("BOM001"),
          ProductCode = new ProductCode("PRODUCT001"),
          BomName = "Test BOM"
        };
        _dbContext.BomRecipes.Add(bomRecipe);
        await _dbContext.SaveChangesAsync();

        var createDto = new CreateWorkOrderDto
        {
          Code = "WO-001",
          BomRecipeId = bomRecipe.Id,
          IsInfinite = false,
          WorkOrderAmount = 100,
          PerTraceInfo = 1
        };

        // Act
        var result = await _workOrderService.CreateWorkOrderAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WO-001", result.Code);
        Assert.Equal(bomRecipe.Id, result.BomRecipeId);
        Assert.Equal("Test BOM", result.BomRecipeName);
        Assert.Equal(100, result.WorkOrderAmount);
      }
      finally
      {
        // 清理测试环境
        CleanupTest();
      }
    }

    [Fact]
    public async Task GetWorkOrderByIdAsync_WithValidId_ShouldReturnWorkOrderDto()
    {
      // 初始化测试环境
      InitializeTest();

      try
      {
        // Arrange
        // 先创建BOM配方
        var bomRecipe = new BomRecipe
        {
          BomCode = new BomCode("BOM001"),
          ProductCode = new ProductCode("PRODUCT001"),
          BomName = "Test BOM"
        };
        _dbContext.BomRecipes.Add(bomRecipe);
        await _dbContext.SaveChangesAsync();

        // 使用服务方法创建工单
        var createDto = new CreateWorkOrderDto
        {
          Code = "WO-001",
          BomRecipeId = bomRecipe.Id,
          IsInfinite = false,
          WorkOrderAmount = 100,
          PerTraceInfo = 1
        };
        var createdWorkOrder = await _workOrderService.CreateWorkOrderAsync(createDto);

        // Act
        var result = await _workOrderService.GetWorkOrderByIdAsync(createdWorkOrder.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdWorkOrder.Id, result.Id);
        Assert.Equal("WO-001", result.Code);
        Assert.Equal("Test BOM", result.BomRecipeName);
      }
      finally
      {
        // 清理测试环境
        CleanupTest();
      }
    }

    [Fact]
    public async Task GetWorkOrderByCodeAsync_WithValidCode_ShouldReturnWorkOrderDto()
    {
      // 初始化测试环境
      InitializeTest();

      try
      {
        // Arrange
        // 先创建BOM配方
        var bomRecipe = new BomRecipe
        {
          BomCode = new BomCode("BOM001"),
          ProductCode = new ProductCode("PRODUCT001"),
          BomName = "Test BOM"
        };
        _dbContext.BomRecipes.Add(bomRecipe);
        await _dbContext.SaveChangesAsync();

        // 使用服务方法创建工单
        var createDto = new CreateWorkOrderDto
        {
          Code = "WO-001",
          BomRecipeId = bomRecipe.Id,
          IsInfinite = false,
          WorkOrderAmount = 100,
          PerTraceInfo = 1
        };
        await _workOrderService.CreateWorkOrderAsync(createDto);

        // Act
        var result = await _workOrderService.GetWorkOrderByCodeAsync("WO-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WO-001", result.Code);
      }
      finally
      {
        // 清理测试环境
        CleanupTest();
      }
    }

    [Fact]
    public async Task UpdateWorkOrderAsync_WithValidData_ShouldReturnUpdatedWorkOrderDto()
    {
      // 初始化测试环境
      InitializeTest();

      try
      {
        // Arrange
        // 创建两个BOM配方
        var oldBomRecipe = new BomRecipe
        {
          BomCode = new BomCode("BOM001"),
          ProductCode = new ProductCode("PRODUCT001"),
          BomName = "Old BOM"
        };
        _dbContext.BomRecipes.Add(oldBomRecipe);

        var newBomRecipe = new BomRecipe
        {
          BomCode = new BomCode("BOM002"),
          ProductCode = new ProductCode("PRODUCT002"),
          BomName = "Updated BOM"
        };
        _dbContext.BomRecipes.Add(newBomRecipe);
        await _dbContext.SaveChangesAsync();

        // 创建工单
        var createDto = new CreateWorkOrderDto
        {
          Code = "WO-001",
          BomRecipeId = oldBomRecipe.Id,
          IsInfinite = false,
          WorkOrderAmount = 100,
          PerTraceInfo = 1
        };
        var createdWorkOrder = await _workOrderService.CreateWorkOrderAsync(createDto);

        // 准备更新数据
        var updateDto = new UpdateWorkOrderDto
        {
          Id = createdWorkOrder.Id,
          BomRecipeId = newBomRecipe.Id,
          IsInfinite = true,
          WorkOrderAmount = 200,
          PerTraceInfo = 2
        };

        // Act
        var result = await _workOrderService.UpdateWorkOrderAsync(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdWorkOrder.Id, result.Id);
        Assert.Equal(newBomRecipe.Id, result.BomRecipeId);
        Assert.True(result.IsInfinite);
        Assert.Equal("Updated BOM", result.BomRecipeName);
      }
      finally
      {
        // 清理测试环境
        CleanupTest();
      }
    }

    [Fact]
    public async Task DeleteWorkOrderAsync_WithValidId_ShouldReturnTrue()
    {
      // 初始化测试环境
      InitializeTest();

      try
      {
        // Arrange
        // 创建BOM配方
        var bomRecipe = new BomRecipe
        {
          BomCode = new BomCode("BOM001"),
          ProductCode = new ProductCode("PRODUCT001"),
          BomName = "Test BOM"
        };
        _dbContext.BomRecipes.Add(bomRecipe);
        await _dbContext.SaveChangesAsync();

        // 创建工单
        var createDto = new CreateWorkOrderDto
        {
          Code = "WO-001",
          BomRecipeId = bomRecipe.Id,
          IsInfinite = false,
          WorkOrderAmount = 100,
          PerTraceInfo = 1
        };
        var createdWorkOrder = await _workOrderService.CreateWorkOrderAsync(createDto);

        // Act
        var result = await _workOrderService.DeleteWorkOrderAsync(createdWorkOrder.Id);

        // Assert
        Assert.True(result);

        // 验证工单确实被删除了
        var deletedWorkOrder = await _workOrderService.GetWorkOrderByIdAsync(createdWorkOrder.Id);
        Assert.Null(deletedWorkOrder);
      }
      finally
      {
        // 清理测试环境
        CleanupTest();
      }
    }

    [Fact]
    public async Task DeleteWorkOrderAsync_WithInvalidId_ShouldReturnFalse()
    {
      // 初始化测试环境
      InitializeTest();

      try
      {
        // Act - 删除不存在的工单
        var result = await _workOrderService.DeleteWorkOrderAsync(999);

        // Assert
        Assert.False(result);
      }
      finally
      {
        // 清理测试环境
        CleanupTest();
      }
    }

    [Fact]
    public async Task GetWorkOrdersAsync_WithValidQuery_ShouldReturnPaginatedList()
    {
      // 初始化测试环境
      InitializeTest();

      try
      {
        // Arrange
        // 创建BOM配方
        var bomRecipe = new BomRecipe
        {
          BomCode = new BomCode("BOM001"),
          ProductCode = new ProductCode("PRODUCT001"),
          BomName = "Test BOM"
        };
        _dbContext.BomRecipes.Add(bomRecipe);
        await _dbContext.SaveChangesAsync();

        // 创建两个工单
        var createDto1 = new CreateWorkOrderDto
        {
          Code = "WO-001",
          BomRecipeId = bomRecipe.Id,
          IsInfinite = false,
          WorkOrderAmount = 100,
          PerTraceInfo = 1
        };
        await _workOrderService.CreateWorkOrderAsync(createDto1);

        var createDto2 = new CreateWorkOrderDto
        {
          Code = "WO-002",
          BomRecipeId = bomRecipe.Id,
          IsInfinite = true,
          WorkOrderAmount = 200,
          PerTraceInfo = 1
        };
        await _workOrderService.CreateWorkOrderAsync(createDto2);

        var queryDto = new WorkOrderQueryDto
        {
          Current = 1,
          PageSize = 10
        };

        // Act
        var result = await _workOrderService.GetWorkOrdersAsync(queryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCounts);
        Assert.Equal(1, result.PageIndex);
        Assert.Equal(2, result.Count); // Count 是 List<T> 的属性，表示当前页的项目数
      }
      finally
      {
        // 清理测试环境
        CleanupTest();
      }
    }
  }
}