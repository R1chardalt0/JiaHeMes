using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.DbContexts.Repository;
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

        public MyFixture()
        {
            WorkOrderRepositoryMock = new Mock<IRepository<WorkOrder>>();
            WorkOrderExecutionRepositoryMock = new Mock<IRepository<WorkOrderExecution>>();
            BomRecipeRepositoryMock = new Mock<IRepository<BomRecipe>>();
        }

        public void Dispose()
        {
            // Clean up resources if needed
        }
    }
}