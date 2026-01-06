using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;

namespace ChargePadLine.Service.Trace.Impl
{
    /// <summary>
    /// 工单服务实现
    /// </summary>
    public class CommonInterfaseService : ICommonInterfaseService
    {
        private readonly IRepository<WorkOrder> _workOrderRepo;
        private readonly IRepository<BomRecipe> _bomRecipeRepo;
        private readonly IRepository<Material> _materialRepo;
        private readonly ILogger<WorkOrderService> _logger;
        private readonly AppDbContext _dbContext;

        public CommonInterfaseService(
            IRepository<WorkOrder> workOrderRepo,
            IRepository<BomRecipe> bomRecipeRepo,
            IRepository<Material> materialRepo,
            ILogger<WorkOrderService> logger,
            AppDbContext dbContext)
        {
            _workOrderRepo = workOrderRepo;
            _bomRecipeRepo = bomRecipeRepo;
            _materialRepo = materialRepo;
            _logger = logger;
            _dbContext = dbContext;
        }

        public Task<string> GetName()
        {
            return null;
        }

        public Task<List<string>> GetList()
        {
            throw new NotImplementedException();
        }

        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadCheck(RequestUploadCheckParams request)
        {
            //根据资源号获取工单
            var DeviceInfosList = _dbContext.DeviceInfos.FirstOrDefault(x => x.Resource == request.Resource);
            if (DeviceInfosList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
            }
            //检查设备是否绑定工单
            if (DeviceInfosList.WorkOrderCode == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
            }
            //检查工单状态
            if (DeviceInfosList.WorkOrderCode != null)
            {
                var workOrder = _dbContext.WorkOrders.FirstOrDefault(x => x.Code.Value == DeviceInfosList.WorkOrderCode);
                if (workOrder == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{DeviceInfosList.WorkOrderCode}"));
                }
                //if (workOrder.Status != "进行中")
                //{
                //    throw new InvalidOperationException($"工单状态错误，工单编号：{DeviceInfosList.WorkOrderCode}");
                //}
            }
            return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"检查通过"));
        }
    }
}