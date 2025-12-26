using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ChargePadLine.Application.Trace.Production.BatchQueue;
using ChargePadLine.Application.Trace.Production.Recipes;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Production.BatchQueue;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using Microsoft.FSharp.Core;
using Moq;
using Xunit;

namespace ChargePadLine.Tests
{
    public class BatchQueueTest : IClassFixture<MyFixture>
    {
        private readonly MyFixture _fixture;
        private readonly MaterialBatchQueueBiz _materialBatchQueueBiz;

        public BatchQueueTest(MyFixture fixture)
        {
            _fixture = fixture;
            _materialBatchQueueBiz = new MaterialBatchQueueBiz(
                _fixture.MaterialBatchQueueItemRepoMock.Object,
                _fixture.BomRecipeSpecialRepositoryMock.Object
            );
        }

        #region 上料 Tests

        [Fact]
        public async Task MapInputToCmdArgAsync_上料_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_上料("BOM001", "BATCH001", 100, 1);
            var bomItem = new BomRecipeItem { BomItemCode = "BOM001" };
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindBomItemByCodeWithNoTrackAsync("BOM001"))
                .ReturnsAsync(bomItem);

            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.CheckBatchExistsAsync("BOM001", "BATCH001"))
                .ReturnsAsync((BatchMaterialQueueItem)null);

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(input.BomItemCode, okResult.BomItemCode);
            Assert.Equal(input.BatchCode, okResult.BatchCode);
            Assert.Equal(input.Amount, okResult.Amount);
            Assert.Equal(input.Priority, okResult.Priority);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_上料_WithInvalidAmount_ReturnsError()
        {
            // Arrange
            var input = new Input_物料批_上料("BOM001", "BATCH001", 0, 1); // 0 amount is invalid

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<IErr_批次上料.Err_批次上料_数量非法>(errorResult);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_上料_WithNonExistentBomItem_ReturnsError()
        {
            // Arrange
            var input = new Input_物料批_上料("NONEXISTENT", "BATCH001", 100, 1);
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindBomItemByCodeWithNoTrackAsync("NONEXISTENT"))
                .ReturnsAsync((BomRecipeItem)null);

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<IErr_批次上料.Err_批次上料_BomItem不存在>(errorResult);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_上料_WithDuplicateBatchCode_ReturnsError()
        {
            // Arrange
            var input = new Input_物料批_上料("BOM001", "BATCH001", 100, 1);
            var bomItem = new BomRecipeItem { BomItemCode = "BOM001" };
            var existingBatch = new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", BatchCode = "BATCH001" };
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindBomItemByCodeWithNoTrackAsync("BOM001"))
                .ReturnsAsync(bomItem);

            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.CheckBatchExistsAsync("BOM001", "BATCH001"))
                .ReturnsAsync(existingBatch);

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<IErr_批次上料.Err_批次上料_批次重复>(errorResult);
        }

        [Fact]
        public async Task 上料Async_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_上料("BOM001", "BATCH001", 100, 1);
            var bomItem = new BomRecipeItem { BomItemCode = "BOM001" };
            var newBatchItem = new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", BatchCode = "BATCH001", RemainingAmount = 100, Priority = 1 };
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindBomItemByCodeWithNoTrackAsync("BOM001"))
                .ReturnsAsync(bomItem);

            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.CheckBatchExistsAsync("BOM001", "BATCH001"))
                .ReturnsAsync((BatchMaterialQueueItem)null);

            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.AddAsync(newBatchItem, true))
                .ReturnsAsync(newBatchItem);

            // Act
            var result = await _materialBatchQueueBiz.上料Async(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal("BOM001", okResult.BomItemCode);
            Assert.Equal("BATCH001", okResult.BatchCode);
            Assert.Equal(100, okResult.RemainingAmount);
        }

        #endregion

        #region 扣料 Tests

        [Fact]
        public async Task MapInputToCmdArgAsync_扣料_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_扣料("BOM001", 50);
            var candidates = new List<BatchMaterialQueueItem>
            {
                new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", RemainingAmount = 100 }
            };
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindTopCandicatesAsync("BOM001", 4))
                .ReturnsAsync(candidates);

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(input.BomItemCode, okResult.BomItemCode);
            Assert.Equal(input.RequiredAmount, okResult.RequiredAmount);
            Assert.Equal(candidates, okResult.Candicates);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_扣料_WithInvalidAmount_ReturnsError()
        {
            // Arrange
            var input = new Input_物料批_扣料("BOM001", 0); // 0 amount is invalid

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<IErr_批次扣料.Err_批次扣料_扣除量非法>(errorResult);
        }

        [Fact]
        public async Task 扣料Async_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_扣料("BOM001", 50);
            var candidates = new List<BatchMaterialQueueItem>
            {
                new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", RemainingAmount = 100 }
            };
            var deductionResult = new List<扣料描述> { new 扣料描述(1, "BOM001", 0, "", 50) };
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindTopCandicatesAsync("BOM001", 4))
                .ReturnsAsync(candidates);

            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _materialBatchQueueBiz.扣料Async(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Single(okResult);
        }

        #endregion

        #region 指定批次单一扣料 Tests

        [Fact]
        public async Task GetRequiredBatchMaterialQueueItemAsync_WithValidBatchCode_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_指定批次单一扣料("BOM001", "BATCH001");
            var candidates = new List<BatchMaterialQueueItem>
            {
                new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", BatchCode = "BATCH001", RemainingAmount = 100 }
            };
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindTopCandicatesAsync("BOM001", 1))
                .ReturnsAsync(candidates);

            // Act
            var result = await _materialBatchQueueBiz.GetRequiredBatchMaterialQueueItemAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal("BOM001", okResult.BomItemCode);
            Assert.Equal("BATCH001", okResult.BatchCode);
        }

        [Fact]
        public async Task GetRequiredBatchMaterialQueueItemAsync_WithWrongBatchCode_ReturnsError()
        {
            // Arrange
            var input = new Input_物料批_指定批次单一扣料("BOM001", "BATCH001");
            var candidates = new List<BatchMaterialQueueItem>
            {
                new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", BatchCode = "WRONGBATCH", RemainingAmount = 100 }
            };
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindTopCandicatesAsync("BOM001", 1))
                .ReturnsAsync(candidates);

            // Act
            var result = await _materialBatchQueueBiz.GetRequiredBatchMaterialQueueItemAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<IErr_批次扣料.Err_批次扣料_指定批次错误>(errorResult);
        }

        [Fact]
        public async Task GetRequiredBatchMaterialQueueItemAsync_WithNoCandidates_ReturnsError()
        {
            // Arrange
            var input = new Input_物料批_指定批次单一扣料("BOM001", "BATCH001");
            var candidates = new List<BatchMaterialQueueItem>();
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindTopCandicatesAsync("BOM001", 1))
                .ReturnsAsync(candidates);

            // Act
            var result = await _materialBatchQueueBiz.GetRequiredBatchMaterialQueueItemAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<IErr_批次扣料.Err_批次扣料_超额>(errorResult);
        }

        [Fact]
        public async Task 扣料Async_指定批次单一扣料_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_指定批次单一扣料("BOM001", "BATCH001");
            var batchItem = new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", BatchCode = "BATCH001", RemainingAmount = 100 };
            var candidates = new List<BatchMaterialQueueItem> { batchItem };
            var deductionResult = new 扣料描述(1, "BOM001", 0, "BATCH001", 1);
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindTopCandicatesAsync("BOM001", 1))
                .ReturnsAsync(candidates);

            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _materialBatchQueueBiz.扣料Async(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
        }

        #endregion

        #region 调整优先级 Tests

        [Fact]
        public async Task MapInputToCmdArgAsync_调整优先级_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_调整优先级(1, 5);
            var batchItem = new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", BatchCode = "BATCH001", RemainingAmount = 100, Priority = 1 };
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindAsync(1))
                .ReturnsAsync(batchItem);

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(batchItem, okResult.QueueItem);
            Assert.Equal(5, okResult.Priority);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_调整优先级_WithNonExistentItem_ReturnsError()
        {
            // Arrange
            var input = new Input_物料批_调整优先级(999, 5);
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindAsync(999))
                .ReturnsAsync((BatchMaterialQueueItem)null);

            // Act
            var result = await _materialBatchQueueBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<Err_物料批_Misc>(errorResult);
        }

        [Fact]
        public async Task 调整优先级Async_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_物料批_调整优先级(1, 5);
            var batchItem = new BatchMaterialQueueItem { Id = 1, BomItemCode = "BOM001", BatchCode = "BATCH001", RemainingAmount = 100, Priority = 1 };
            
            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.FindAsync(1))
                .ReturnsAsync(batchItem);

            _fixture.MaterialBatchQueueItemRepoMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _materialBatchQueueBiz.调整优先级Async(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(batchItem, okResult);
        }

        #endregion
    }
}