using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Application.Trace.Production.BatchQueue;
using ChargePadLine.Application.Trace.Production.Recipes;
using ChargePadLine.Application.Trace.Production.TraceInformation;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using Moq;

namespace ChargePadLine.Tests
{
    public class MyFixture : IDisposable
    {
        public Mock<IRepository<WorkOrder>> WorkOrderRepositoryMock { get; }
        public Mock<IRepository<WorkOrderExecution>> WorkOrderExecutionRepositoryMock { get; }
        public Mock<IRepository<BomRecipe>> BomRecipeRepositoryMock { get; }
        public Mock<IBomRecipeRepository> BomRecipeSpecialRepositoryMock { get; }
        public Mock<IMaterialBatchQueueItemRepo> MaterialBatchQueueItemRepoMock { get; }
        public Mock<IMaterialRepository> MaterialRepositoryMock { get; }
        public Mock<ICtrlVsnsService> CtrlVsnsServiceMock { get; }

        public MyFixture()
        {
            WorkOrderRepositoryMock = new Mock<IRepository<WorkOrder>>();
            WorkOrderExecutionRepositoryMock = new Mock<IRepository<WorkOrderExecution>>();
            BomRecipeRepositoryMock = new Mock<IRepository<BomRecipe>>();
            BomRecipeSpecialRepositoryMock = new Mock<IBomRecipeRepository>();
            MaterialBatchQueueItemRepoMock = new Mock<IMaterialBatchQueueItemRepo>();
            MaterialRepositoryMock = new Mock<IMaterialRepository>();
            CtrlVsnsServiceMock = new Mock<ICtrlVsnsService>();
        }

        public void Dispose()
        {
            // Clean up resources if needed
        }
    }
}
