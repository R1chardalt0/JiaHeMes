using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargePadLine.Application.Trace.Production.TraceInformation;
using ChargePadLine.Application.Trace.Production.Traceinfo;
using ChargePadLine.Application.Trace.Production.WorkOrders;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ChargePadLine.Common.Config;
using ChargePadLine.Application.Trace.Production.BatchQueue;
using Moq;
using Xunit;

namespace ChargePadLine.Tests
{
    public class TraceInfoIntegrationTest : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly TraceInfoBiz _traceInfoBiz;
        private readonly AppDbContext _context;

        public TraceInfoIntegrationTest(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _context = fixture.Context;
            
            _traceInfoBiz = new TraceInfoBiz(
                Options.Create(fixture.AppOpts),
                fixture.TraceInfoRepository,
                fixture.WorkOrderRepository,
                fixture.WorkOrderExecutionRepository,
                fixture.CtrlVsnsService
            );
        }

        [Fact]
        public async Task AddTraceInfoMainAsync_WithValidData_ShouldSaveToDatabase()
        {
            // Arrange - 创建真实的测试数据
            var workOrder = new WorkOrder 
            { 
                Id = 2001, 
                Code = new WorkOrderCode("WO_TEST_001"), 
                ProductCode = new ProductCode("PRODUCT_TEST"),
                DocStatus = WorkOrderDocStatus.Approved,
                BomRecipeId = 1
            };
            
            var workOrderExecution = new WorkOrderExecution 
            { 
                Id = 1, 
                WorkOrderCode = new WorkOrderCode("WO_TEST_001"),
                WorkOrder = workOrder,
                WorkOrderId = workOrder.Id
            };
            
            var vsn = new CtrlVsn { Id = 1, ProductCode = "PRODUCT_TEST", Current = 1000 };

            // 先保存基础数据到数据库
            _context.WorkOrders.Add(workOrder);
            _context.WorkOrderExecutions.Add(workOrderExecution);
            await _context.SaveChangesAsync();

            var arg = new CmdArg_TraceInfo_AddMain(workOrderExecution, "LINE_TEST", vsn);

            // Act - 执行真实操作
            var result = await _traceInfoBiz.AddTraceInfoMainAsync(arg);

            // Assert - 验证数据确实保存到数据库
            Assert.True(result.IsOk);
            var addedTraceInfo = result.ResultValue;
            Assert.NotNull(addedTraceInfo);

            // 从数据库中验证数据
            var traceInfoFromDb = await _context.TraceInfos
                .Include(t => t.WorkOrder)
                .Include(t => t.BomItems)
                .FirstOrDefaultAsync(t => t.Id == addedTraceInfo.Id);
                
            Assert.NotNull(traceInfoFromDb);
            Assert.Equal<uint>(1000, traceInfoFromDb.Vsn);
            Assert.Equal("LINE_TEST", traceInfoFromDb.ProductLine);
            Assert.Equal(workOrder.Id, traceInfoFromDb.WorkOrderId);

            // 清理测试数据
            _context.TraceInfos.Remove(traceInfoFromDb);
            _context.WorkOrderExecutions.Remove(workOrderExecution);
            _context.WorkOrders.Remove(workOrder);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task AddBomItemAsync_WithValidData_ShouldSaveToDatabase()
        {
            // Arrange - 创建测试数据
            var traceInfo = new TraceInfo 
            { 
                Id = Guid.NewGuid(), 
                Vsn = 1002, 
                ProductLine = "LINE_TEST",
                WorkOrderId = 2001,
                CreatedAt = DateTimeOffset.UtcNow,
                BomItems = new List<TraceBomItem>()
            };
            
            var bomRecipeItem = new BomRecipeItem 
            { 
                Id = 1, 
                BomItemCode = new BomItemCode("BOM_ITEM_TEST"), 
                Quota = 15.0m,
                MaterialCode = new MaterialCode("MAT_TEST"),
                MeasureUnit = new MeasureUnit("PC"),
                MaterialName = "Test Material",
                Description = "Test Description",
                BomId = 1
            };

            // 保存到数据库
            _context.TraceInfos.Add(traceInfo);
            await _context.SaveChangesAsync();

            var arg = new CmdArg_TraceInfo_AddBomItem(
                traceInfo, 
                bomRecipeItem, 
                "SKU_TEST", 
                10.5m
            );

            // Act
            var result = await _traceInfoBiz.AddBomItemAsync(arg);

            // Assert
            Assert.True(result.IsOk);

            // 验证数据保存到数据库
            var traceInfoFromDb = await _context.TraceInfos
                .Include(t => t.BomItems)
                .FirstOrDefaultAsync(t => t.Id == traceInfo.Id);
                
            Assert.NotNull(traceInfoFromDb);
            Assert.Single(traceInfoFromDb.BomItems);
            Assert.Equal("BOM_ITEM_TEST", traceInfoFromDb.BomItems[0].BomItemCode.Value);

            // 清理测试数据
            _context.TraceInfos.Remove(traceInfoFromDb);
            await _context.SaveChangesAsync();
        }
    }

    public class IntegrationTestFixture : IDisposable
    {
        public AppDbContext Context { get; }
        public ITraceInfoRepository TraceInfoRepository { get; }
        public IWorkOrderRepository WorkOrderRepository { get; }
        public IWorkOrderExecutionRepository WorkOrderExecutionRepository { get; }
        public ICtrlVsnsService CtrlVsnsService { get; }
        public AppOpt AppOpts { get; }

        public IntegrationTestFixture()
        {
            // 创建内存数据库用于测试
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
                
            // 创建虚拟IHostEnvironment
            var mockEnv = new Mock<IHostEnvironment>();
            mockEnv.Setup(x => x.EnvironmentName).Returns("Testing");
            
            Context = new AppDbContext(options, mockEnv.Object);
            
            // 创建真实Repository实例
            var repository = new Repository<TraceInfo>(Context);
            TraceInfoRepository = new TraceInfoRepository(repository, Context);
            
            var workOrderRepository = new Repository<WorkOrder>(Context);
            WorkOrderRepository = new WorkOrderRepository(workOrderRepository, Context);
            
            var workOrderExecutionRepository = new Repository<WorkOrderExecution>(Context);
            WorkOrderExecutionRepository = new WorkOrderExecutionRepository(workOrderExecutionRepository, Context);
            
            // Mock CtrlVsnsService（这个通常需要外部系统）
            var ctrlVsnsMock = new Mock<ICtrlVsnsService>();
            ctrlVsnsMock.Setup(x => x.TryGetVsnAsync(It.IsAny<string>()))
                .ReturnsAsync(new CtrlVsn { Id = 1, ProductCode = "PRODUCT_TEST", Current = 1000 });
            CtrlVsnsService = ctrlVsnsMock.Object;
            
            AppOpts = new AppOpt 
            { 
                ProductLineCode = "LINE001"
            };
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}