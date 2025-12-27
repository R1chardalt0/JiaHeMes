using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargePadLine.Application.Trace.Production.TraceInformation;
using ChargePadLine.Application.Trace.Production.WorkOrders;
using ChargePadLine.Application.Trace.Production.WorkOrders.Errors;
using ChargePadLine.Application.Trace.Production.Traceinfo;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Application.Trace.Production.TraceInformation;
using Microsoft.FSharp.Core;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using ChargePadLine.Common.Config;

namespace ChargePadLine.Tests
{
    public class TraceInfoTest : IClassFixture<TraceInfoFixture>
    {
        private readonly TraceInfoFixture _fixture;
        private readonly TraceInfoBiz _traceInfoBiz;

        public TraceInfoTest(TraceInfoFixture fixture)
        {
            _fixture = fixture;
            _traceInfoBiz = new TraceInfoBiz(
                Options.Create(_fixture.AppOpts),
                _fixture.TraceInfoRepositoryMock.Object,
                _fixture.WorkOrderRepositoryMock.Object,
                _fixture.WorkOrderExecutionRepositoryMock.Object,
                _fixture.CtrlVsnsServiceMock.Object
            );
        }

        #region 工单生产-增加TraceInfo主表测试

        [Fact]
        public async Task MapInputToCmdArgAsync_AddMain_WithValidWorkOrderId_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_AddMain(1001);
            var workOrder = new WorkOrder 
            { 
                Id = 1001, 
                Code = new WorkOrderCode("WO2024001"), 
                ProductCode = new ProductCode("PRODUCT001"),
                DocStatus = WorkOrderDocStatus.Approved 
            };
            var workOrderExecution = new WorkOrderExecution 
            { 
                Id = 1, 
                WorkOrderCode = new WorkOrderCode("WO2024001"),
                WorkOrder = workOrder 
            };
            var vsn = new CtrlVsn { Id = 1, ProductCode = "PRODUCT001", Current = 1000 };

            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.FindAsync(1001))
                .ReturnsAsync(workOrder);

            _fixture.WorkOrderExecutionRepositoryMock
                .Setup(x => x.FindWithWorOrderCodeAsync(new WorkOrderCode("WO2024001")))
                .ReturnsAsync(workOrderExecution);

            _fixture.CtrlVsnsServiceMock
                .Setup(x => x.TryGetVsnAsync("PRODUCT001"))
                .ReturnsAsync(vsn);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal(workOrder, cmdArg.WorkOrderExecution.WorkOrder);
            Assert.Equal("LINE001", cmdArg.ProductLineCode);
            Assert.Equal(vsn, cmdArg.CtrlVsn);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_AddMain_WithInvalidWorkOrderId_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_AddMain(9999);

            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.FindAsync(9999))
                .ReturnsAsync((WorkOrder?)null);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_AddMain_WithInvalidWorkOrderExecution_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_AddMain(1001);
            var workOrder = new WorkOrder 
            { 
                Id = 1001, 
                Code = new WorkOrderCode("WO2024001"), 
                ProductCode = new ProductCode("PRODUCT001"),
                DocStatus = WorkOrderDocStatus.Approved 
            };

            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.FindAsync(1001))
                .ReturnsAsync(workOrder);

            _fixture.WorkOrderExecutionRepositoryMock
                .Setup(x => x.FindWithWorOrderCodeAsync(new WorkOrderCode("WO2024001")))
                .ReturnsAsync((WorkOrderExecution?)null);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task AddTraceInfoMainAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var workOrder = new WorkOrder 
            { 
                Id = 1001, 
                Code = new WorkOrderCode("WO2024001"), 
                ProductCode = new ProductCode("PRODUCT001"),
                DocStatus = WorkOrderDocStatus.Approved 
            };
            var workOrders = new WorkOrder
            {
                Id = 1,
                Code = new WorkOrderCode("WO2024001"),
                DocStatus = WorkOrderDocStatus.Approved 
            };
            var workOrderExecution = new WorkOrderExecution 
            { 
                Id = 1, 
                WorkOrderCode = new WorkOrderCode("WO2024001"),
                WorkOrder = workOrders 
            };
            var vsn = new CtrlVsn { Id = 1, ProductCode = "PRODUCT001", Current = 1000 };
            var arg = new CmdArg_TraceInfo_AddMain(workOrderExecution, "LINE001", vsn);
            var traceInfo = new TraceInfo 
            { 
                Id = Guid.NewGuid(), 
                Vsn = 1001, 
                ProductLine = "LINE001",
                WorkOrderId = 1,
                WorkOrder = workOrders
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<TraceInfo>(), true))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.AddTraceInfoMainAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var addedTraceInfo = result.ResultValue;
            Assert.NotNull(addedTraceInfo);
            Assert.Equal<uint>(1000, addedTraceInfo.Vsn);
            Assert.Equal<string>("LINE001", addedTraceInfo.ProductLine);
        }

        #endregion

        #region 工单生产-绑定PIN测试

        [Fact]
        public async Task MapInputToCmdArgAsync_BindPIN_WithValidTraceInfoId_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_BindPIN(Guid.NewGuid(), "PIN123456789");
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal(traceInfo, cmdArg.TraceInfo);
            Assert.Equal("PIN123456789", cmdArg.PIN.Value);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_BindPIN_WithInvalidTraceInfoId_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_BindPIN(Guid.NewGuid(), "PIN123456789");

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync((TraceInfo?)null);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task BindPinAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var traceInfo = new TraceInfo 
            { 
                Id = Guid.NewGuid(), 
                Vsn = 1001, 
                ProductLine = "LINE001",
                BomItems = new List<TraceBomItem>(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var arg = new CmdArg_TraceInfo_BindPIN(traceInfo, new SKU("PIN123456789"));

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _traceInfoBiz.BindPinAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var boundTraceInfo = result.ResultValue;
            Assert.NotNull(boundTraceInfo);
            Assert.Equal(traceInfo.Id, boundTraceInfo.Id);
        }

        #endregion

        #region 增加BOM生产子项测试

        [Fact]
        public async Task MapInputToCmdArgAsync_AddBomItem_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_AddBomItem(
                Guid.NewGuid(), 
                "BOM_ITEM_001", 
                "SKU001", 
                10.5m
            );
            var bomRecipeItem = new BomRecipeItem 
            { 
                Id = 1, 
                BomItemCode = new BomItemCode("BOM_ITEM_001"), 
                Quota = 15.0m,
                MaterialCode = new MaterialCode("MAT001"),
                MeasureUnit = new MeasureUnit("PC"),
                MaterialName = "Test Material",
                Description = "Test Description"
            };
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                BomItems = new List<TraceBomItem>
                {
                    new TraceBomItem
                    {
                        Id = Guid.NewGuid(),
                        TraceInfoId = input.TraceInfoId,
                        TraceInfo = null,
                        MaterialCode = "MAT001",
                        BomItemCode = "BOM_ITEM_001",
                        MeasureUnit = "PC",
                        MaterialName = "Test Material",
                        Description = "Test Description",
                        Quota = 20.0m,
                        Consumption = 10.0m,
                        IsDeleted = false,
                        CreatedAt = DateTimeOffset.UtcNow
                    }
                },
                CreatedAt = DateTimeOffset.UtcNow,
                BomRecipe = new BomRecipe
                {
                    Id = 1,
                    Items = new List<BomRecipeItem> { bomRecipeItem }
                }
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal("BOM_ITEM_001", cmdArg.Recipe.BomItemCode.Value);
            Assert.Equal("SKU001", cmdArg.SKU.Value);
            Assert.Equal(10.5m, cmdArg.Consumption);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_AddBomItem_WithInvalidTraceInfoId_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_AddBomItem(
                Guid.NewGuid(), 
                "BOM_ITEM_001", 
                "SKU001", 
                10.5m
            );

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync((TraceInfo?)null);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task AddBomItemAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var traceInfo = new TraceInfo 
            { 
                Id = Guid.NewGuid(), 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow,
                BomItems = new List<TraceBomItem>()
            };
            var bomRecipeItem = new BomRecipeItem 
            { 
                Id = 1, 
                BomItemCode = new BomItemCode("BOM_ITEM_001"), 
                Quota = 15.0m,
                MaterialCode = new MaterialCode("MAT001"),
                MeasureUnit = new MeasureUnit("PC"),
                MaterialName = "Test Material",
                Description = "Test Description",
                BomId = 1
            };
            var arg = new CmdArg_TraceInfo_AddBomItem(
                traceInfo, 
                bomRecipeItem, 
                "SKU001", 
                10.5m
            );

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _traceInfoBiz.AddBomItemAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var updatedTraceInfo = result.ResultValue;
            Assert.NotNull(updatedTraceInfo);
        }

        #endregion

        #region 删除BOM生产子项测试

        [Fact]
        public async Task MapInputToCmdArgAsync_RemoveBomItem_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_RemoveBomItem(Guid.NewGuid(), Guid.NewGuid());
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                BomItems = new List<TraceBomItem>
                {
                    new TraceBomItem { Id = input.BomItemId, BomItemCode = new BomItemCode("BOM_ITEM_001") }
                }
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal(traceInfo, cmdArg.TraceInfo);
            Assert.NotNull(cmdArg.ProdBomItem);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_RemoveBomItem_WithInvalidBomItemId_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_RemoveBomItem(Guid.NewGuid(), Guid.NewGuid());
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                BomItems = new List<TraceBomItem>(), // 空列表
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task RemoveBomItemAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var traceInfoId = Guid.NewGuid();
            var bomItemId = Guid.NewGuid();
            var bomItemCode = new BomItemCode("BOM_ITEM_001");
            
            var traceInfo = new TraceInfo 
            { 
                Id = traceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow,
                BomItems = new List<TraceBomItem>()
            };
            
            var bomItem = new TraceBomItem 
            { 
                Id = bomItemId, 
                TraceInfoId = traceInfoId,
                BomItemCode = bomItemCode,
                IsDeleted = false
            };
            
            traceInfo.BomItems.Add(bomItem);
            var arg = new CmdArg_TraceInfo_RemoveBomItem(traceInfo, bomItem);

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _traceInfoBiz.RemoveBomItemAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var updatedTraceInfo = result.ResultValue;
            Assert.NotNull(updatedTraceInfo);
        }

        #endregion

        #region 增加PROC生产子项测试

        [Fact]
        public async Task MapInputToCmdArgAsync_AddProcItem_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_AddProcItem(
                "PIN123456789",
                "STATION_01",
                "temperature",
                JToken.FromObject(new { value = 25.5, unit = "°C" }),
                false
            );
            var traceInfo = new TraceInfo 
            { 
                Id = Guid.NewGuid(), 
                Vsn = 1001, 
                ProductLine = "LINE001",
                PIN = new SKU("PIN123456789"),
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindWithPINAsync(new SKU("PIN123456789")))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal("STATION_01", cmdArg.Station);
            Assert.Equal("temperature", cmdArg.Key);
            Assert.False(cmdArg.DeleteExisting);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_AddProcItem_WithInvalidPIN_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_AddProcItem(
                "INVALID_PIN",
                "STATION_01",
                "temperature",
                JToken.FromObject(new { value = 25.5, unit = "°C" }),
                false
            );

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindWithPINAsync(new SKU("INVALID_PIN")))
                .ReturnsAsync((TraceInfo?)null);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task AddProcItemAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var traceInfo = new TraceInfo 
            { 
                Id = Guid.NewGuid(), 
                Vsn = 1001, 
                ProductLine = "LINE001",
                PIN = new SKU("PIN123456789"),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var arg = new CmdArg_TraceInfo_AddProcItem(
                traceInfo,
                "STATION_01",
                "temperature",
                JToken.FromObject(new { value = 25.5, unit = "°C" }),
                false
            );

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _traceInfoBiz.AddProcItemAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var updatedTraceInfo = result.ResultValue;
            Assert.NotNull(updatedTraceInfo);
        }

        #endregion

        #region 删除PROC生产子项测试

        [Fact]
        public async Task MapInputToCmdArgAsync_RemoveProcItem_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var procItemId = Guid.NewGuid();
            var input = new Input_TraceInfo_RemoveProcItem(Guid.NewGuid(), procItemId);
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                ProcItems = new List<TraceProcItem>
                {
                    new TraceProcItem { Id = procItemId, Station = "STATION_01", Key = "temperature", CreatedAt = DateTimeOffset.UtcNow }
                },
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal(traceInfo, cmdArg.TraceInfo);
            Assert.NotNull(cmdArg.ProdProcItem);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_RemoveProcItem_WithInvalidProcItemId_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_RemoveProcItem(Guid.NewGuid(), Guid.NewGuid());
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                ProcItems = new List<TraceProcItem>() // 空列表
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task RemoveProcItemAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var traceInfoId = Guid.NewGuid();
            var procItemId = Guid.NewGuid();
            var station = "STATION_01";
            var key = "temperature";
            
            var traceInfo = new TraceInfo 
            { 
                Id = traceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow,
                ProcItems = new List<TraceProcItem>()
            };
            
            var procItem = new TraceProcItem 
            { 
                Id = procItemId, 
                TraceInfoId = traceInfoId,
                Station = station, 
                Key = key,
                IsDeleted = false
            };
            
            traceInfo.ProcItems.Add(procItem);
            var arg = new CmdArg_TraceInfo_RemoveProcItem(traceInfo, procItem);

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _traceInfoBiz.RemoveProcItemAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var updatedTraceInfo = result.ResultValue;
            Assert.NotNull(updatedTraceInfo);
        }

        #endregion

        #region 强制OK/NG测试

        [Fact]
        public async Task MapInputToCmdArgAsync_ForceOkNg_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_ForceOkNg(Guid.NewGuid(), true, "Test NG Reason");
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsyc(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal(traceInfo, cmdArg.TraceInfo);
            Assert.True(cmdArg.IsNg);
            Assert.Equal("Test NG Reason", cmdArg.NgReason);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_ForceOkNg_WithInvalidTraceInfoId_ReturnsError()
        {
            // Arrange
            var input = new Input_TraceInfo_ForceOkNg(Guid.NewGuid(), true, "Test NG Reason");

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync((TraceInfo?)null);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsyc(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        #endregion

        #region 强制破坏测试

        [Fact]
        public async Task MapInputToCmdArgAsync_ForceDestroyed_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_ForceDestroyed(Guid.NewGuid(), true);
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.MapInputToCmdArgAsyc(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal(traceInfo, cmdArg.TraceInfo);
            Assert.True(cmdArg.Destroyed);
        }

        [Fact]
        public async Task 强制破坏kNgAsync_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_ForceDestroyed(Guid.NewGuid(), true);
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _traceInfoBiz.强制破坏kNgAsync(input, true);

            // Assert
            Assert.True(result.IsOk);
            var updatedTraceInfo = result.ResultValue;
            Assert.NotNull(updatedTraceInfo);
        }

        #endregion

        #region 完整流程测试

        [Fact]
        public async Task AddTraceInfoMainAsync_CompleteFlow_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_AddMain(1001);
            var workOrder = new WorkOrder 
            { 
                Id = 1001, 
                Code = new WorkOrderCode("WO2024001"), 
                ProductCode = new ProductCode("PRODUCT001"),
                DocStatus = WorkOrderDocStatus.Approved 
            };
            var workOrderExecution = new WorkOrderExecution 
            { 
                Id = 1, 
                WorkOrderCode = new WorkOrderCode("WO2024001"),
                WorkOrder = workOrder 
            };
            var vsn = new CtrlVsn { Id = 1, ProductCode = "PRODUCT001", Current = 1000 };
            var traceInfo = new TraceInfo 
            { 
                Id = Guid.NewGuid(), 
                Vsn = 1001, 
                ProductLine = "LINE001",
                WorkOrderId = 1,
                WorkOrder = workOrder,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.FindAsync(1001))
                .ReturnsAsync(workOrder);

            _fixture.WorkOrderExecutionRepositoryMock
                .Setup(x => x.FindWithWorOrderCodeAsync(new WorkOrderCode("WO2024001")))
                .ReturnsAsync(workOrderExecution);

            _fixture.CtrlVsnsServiceMock
                .Setup(x => x.TryGetVsnAsync("PRODUCT001"))
                .ReturnsAsync(vsn);

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<TraceInfo>(), true))
                .ReturnsAsync(traceInfo);

            // Act
            var result = await _traceInfoBiz.AddTraceInfoMainAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var addedTraceInfo = result.ResultValue;
            Assert.NotNull(addedTraceInfo);
            Assert.Equal<uint>(1000, addedTraceInfo.Vsn);
            Assert.Equal<string>("LINE001", addedTraceInfo.ProductLine);
        }

        [Fact]
        public async Task BindPinAsync_CompleteFlow_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_TraceInfo_BindPIN(Guid.NewGuid(), "PIN123456789");
            var traceInfo = new TraceInfo 
            { 
                Id = input.TraceInfoId, 
                Vsn = 1001, 
                ProductLine = "LINE001",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.FindAsync(input.TraceInfoId))
                .ReturnsAsync(traceInfo);

            _fixture.TraceInfoRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _traceInfoBiz.BindPinAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var boundTraceInfo = result.ResultValue;
            Assert.NotNull(boundTraceInfo);
            Assert.Equal(traceInfo.Id, boundTraceInfo.Id);
        }

        #endregion
    }

    public class TraceInfoFixture : IDisposable
    {
        public AppOpt AppOpts { get; }
        public Mock<ITraceInfoRepository> TraceInfoRepositoryMock { get; }
        public Mock<IWorkOrderRepository> WorkOrderRepositoryMock { get; }
        public Mock<IWorkOrderExecutionRepository> WorkOrderExecutionRepositoryMock { get; }
        public Mock<ICtrlVsnsService> CtrlVsnsServiceMock { get; }

        public TraceInfoFixture()
        {
            AppOpts = new AppOpt
            {
                ProductLineCode = "LINE001"
            };

            TraceInfoRepositoryMock = new Mock<ITraceInfoRepository>();
            WorkOrderRepositoryMock = new Mock<IWorkOrderRepository>();
            WorkOrderExecutionRepositoryMock = new Mock<IWorkOrderExecutionRepository>();
            CtrlVsnsServiceMock = new Mock<ICtrlVsnsService>();
        }

        public void Dispose()
        {
            // 清理资源
        }
    }
}