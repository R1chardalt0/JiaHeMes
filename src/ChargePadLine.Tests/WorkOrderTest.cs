using System.Linq.Expressions;
using ChargePadLine.Application.Trace.Production.WorkOrders;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.Production;
using Moq;
using Xunit;
using static ChargePadLine.Application.Trace.Production.WorkOrders.Errors.IErr_工单创建;
using static ChargePadLine.Application.Trace.Production.WorkOrders.Errors.IErr_工单启动;
using static ChargePadLine.Application.Trace.Production.WorkOrders.Errors.IErr_工单维护单据;

namespace ChargePadLine.Tests
{
    public class WorkOrderTest : IClassFixture<MyFixture>
    {
        private readonly MyFixture _fixture;
        private readonly WorkOrderBiz _workOrderBiz;

        public WorkOrderTest(MyFixture fixture)
        {
            _fixture = fixture;
            _workOrderBiz = new WorkOrderBiz(
                _fixture.WorkOrderRepositoryMock.Object,
                _fixture.WorkOrderExecutionRepositoryMock.Object,
                _fixture.BomRecipeRepositoryMock.Object
            );
        }

        #region CreateWorkOrder Tests

        [Fact]
        public async Task MapInputToCmdArgAsync_CreateWorkOrder_WithValidBomRecipe_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_WorkOrder_CreateWorkOrder(1, "Test Description", 100m);
            var bomRecipe = new BomRecipe { Id = 1 };
            
            _fixture.BomRecipeRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<BomRecipe, bool>>>()))
                .ReturnsAsync(bomRecipe);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(bomRecipe, okResult.BomRecipe);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_CreateWorkOrder_WithInvalidBomRecipe_ReturnsError()
        {
            // Arrange
                var input = new Input_WorkOrder_CreateWorkOrder(999, "Test Description", 100m);
            
            _fixture.BomRecipeRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<BomRecipe, bool>>>()))
                .ReturnsAsync((BomRecipe)null);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<RecipeNotFound>(errorResult);
        }

        #endregion

        #region StartWorkOrder Tests

        [Fact]
        public async Task MapInputToCmdArgAsync_StartWorkOrder_WithValidWorkOrder_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_WorkOrder_StartWorkOrder(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);
            
            _fixture.WorkOrderExecutionRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrderExecution, bool>>>()))
                .ReturnsAsync((WorkOrderExecution)null);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(workOrder, okResult.WorkOrder);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_StartWorkOrder_WithNonExistentWorkOrder_ReturnsNotFoundError()
        {
            // Arrange
            var input = new Input_WorkOrder_StartWorkOrder(999);
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync((WorkOrder)null);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<NotFound>(errorResult);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_StartWorkOrder_WithAlreadyStartedWorkOrder_ReturnsAlreadyStartedError()
        {
            // Arrange
            var input = new Input_WorkOrder_StartWorkOrder(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            var existingExecution = new WorkOrderExecution { Id = 1, WorkOrderCode = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);
            
            _fixture.WorkOrderExecutionRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrderExecution, bool>>>()))
                .ReturnsAsync(existingExecution);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<AlreadyStarted>(errorResult);
        }

        #endregion

        #region Document Operations Tests

        [Fact]
        public async Task MapInputToCmdArgAsync_CommitDoc_WithValidWorkOrder_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_WorkOrder_CommitDoc(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(workOrder, okResult.WorkOrder);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_CommitDoc_WithNonExistentWorkOrder_ReturnsDocNotFoundError()
        {
            // Arrange
            var input = new Input_WorkOrder_CommitDoc(999);
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync((WorkOrder)null);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.False(result.IsOk);
            var errorResult = result.ErrorValue;
            Assert.IsType<DocNotFound>(errorResult);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_ApproveDoc_WithValidWorkOrder_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_WorkOrder_ApproveDoc(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(workOrder, okResult.WorkOrder);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_RejectDoc_WithValidWorkOrder_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_WorkOrder_RejectDoc(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(workOrder, okResult.WorkOrder);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_DeleteDoc_WithValidWorkOrder_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_WorkOrder_DeleteDoc(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(workOrder, okResult.WorkOrder);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_ReEditDoc_WithValidWorkOrder_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_WorkOrder_ReEditDoc(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var okResult = result.ResultValue;
            Assert.NotNull(okResult);
            Assert.Equal(workOrder, okResult.WorkOrder);
        }

        #endregion

        #region LINQ Extension Method Tests

        [Fact]
        public async Task MapInputToCmdArgAsync_DocumentOperations_VerifyLINQExtensionMethods()
        {
            // This test verifies that the LINQ extension methods (MapNullableToResult, SelectError) work correctly
            
            // Arrange
            var input = new Input_WorkOrder_CommitDoc(1);
            var workOrder = new WorkOrder { Id = 1, Code = new WorkOrderCode("WO-001") };
            
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync(workOrder);

            // Act
            var result = await _workOrderBiz.MapInputToCmdArgAsync(input);

            // Assert - Verify the LINQ query with MapNullableToResult works
            Assert.True(result.IsOk);
            
            // Test error case to verify MapNullableToResult error handling
            _fixture.WorkOrderRepositoryMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<WorkOrder, bool>>>()))
                .ReturnsAsync((WorkOrder)null);
            
            var errorResult = await _workOrderBiz.MapInputToCmdArgAsync(input);
            Assert.False(errorResult.IsOk);
        }

        #endregion
    }
}