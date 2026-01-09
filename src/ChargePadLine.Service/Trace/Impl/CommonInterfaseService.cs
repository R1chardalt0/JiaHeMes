using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.IdentityModel.Tokens;

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
        private readonly ILogger<OrderListService> _logger;
        private readonly AppDbContext _dbContext;

        public CommonInterfaseService(
            IRepository<WorkOrder> workOrderRepo,
            IRepository<BomRecipe> bomRecipeRepo,
            IRepository<Material> materialRepo,
            ILogger<OrderListService> logger,
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
            var workOrder = _dbContext.OrderList.FirstOrDefault(x => x.OrderCode == DeviceInfosList.WorkOrderCode);
            
                
                if (workOrder == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{DeviceInfosList.WorkOrderCode}"));
                }
                
            
            
            var StationList= _dbContext.StationList.FirstOrDefault(x => x.StationCode == request.StationCode);
            if (StationList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
            }

            var processRouteItems = from p in _dbContext.ProcessRoutes
                              join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                              join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                              where p.Id == workOrder.ProcessRouteId
                              orderby iitm.RouteSeq
                              select new {p.RouteCode,p.RouteName,iitm.FirstStation,iitm.MustPassStation,iitm.RouteSeq,iitm.StationCode,iitm.CheckAll,iitm.CheckStationList,s.StationId};
            //  select new { p.RouteCode,iitm. };

            if(processRouteItems.Count() == 0)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线不存在，工单编号：{DeviceInfosList.WorkOrderCode}"));
            }
            var currentprocessRouteList = processRouteItems.Where(x => x.StationCode == request.StationCode).First();
            /// 判断是否是第一站
            if (currentprocessRouteList.FirstStation == true)
            {
                //检查SN规则
                return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"首站"));
            }
            else
            {
                var SNList = _dbContext.mesSnListCurrents.FirstOrDefault(x => x.SnNumber == request.SN);
                if (SNList == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},不存在"));
                }
                if (workOrder.OrderStatus != 3)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单状态不是生产中，工单状态：{workOrder.OrderStatus.ToString()}"));
                }
                if (SNList.CurrentStationListId != StationList.StationId)
                {
                    var SNStationList = _dbContext.StationList.FirstOrDefault(x => x.StationId == SNList.CurrentStationListId);
                    if (SNStationList == null)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不存在"));
                    }
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不一致，SN站点{SNStationList.StationCode}，上传站点{request.StationCode}"));
                }
                if (SNList.IsAbnormal == true)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前状态异常"));
                }
                //var SnListhistory = _dbContext.mesSnListHistories.Where(x => x.SnNumber == request.SN);

                var SnListhistory = from snhis in _dbContext.mesSnListHistories
                      join station in _dbContext.StationList on snhis.CurrentStationListId equals station.StationId
                      where snhis.SnNumber == request.SN
                      select new
                      {
                          SnHistory = snhis,      // 包含mesSnListHistories表的所有列
                          Station = station       // 包含StationList表的所有列
                      };
                ;

                if (currentprocessRouteList.CheckAll == true)
                { 
                    foreach (var processRouteItem in processRouteItems)
                    {
                        if (SnListhistory.Where(x=>x.SnHistory.CurrentStationListId == processRouteItem.StationId && x.SnHistory.StationStatus == 1).Count() == 0)
                        {
                            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{processRouteItem.StationCode}没有PASS记录"));
                        }
                    }
                }
                if (currentprocessRouteList.CheckStationList.IsNullOrEmpty() == false)
                {
                    var checkStationList = currentprocessRouteList.CheckStationList.Split(",");
                    foreach (var stationCode in checkStationList)
                    {
                        if (SnListhistory.Where(x => x.Station.StationCode == stationCode && x.SnHistory.StationStatus == 1).Count() == 0)
                        {
                            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{stationCode}没有PASS记录"));
                        }
                    }
                }
                else
                {
                    // 获取当前工艺路线中当前站点的上一站且为必过站的站点
                    var prevMustPassStation = processRouteItems
                        .Where(x => x.RouteSeq < currentprocessRouteList.RouteSeq && x.MustPassStation == true)
                        .OrderByDescending(x => x.RouteSeq)
                        .FirstOrDefault();
                    if (prevMustPassStation != null)
                    {
                        if (SnListhistory.Where(x => x.SnHistory.CurrentStationListId == prevMustPassStation.StationId && x.SnHistory.StationStatus == 1).Count() == 0)
                        {
                            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},上一必过站点{prevMustPassStation.StationCode}没有PASS记录"));
                        }
                    }

                }
            }



            
            return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"检查通过"));
        }
        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadData(RequestUploadCheckParams request)
        {
            // 先执行与UploadCheck相同的校验逻辑
            var checkResult = await UploadCheck(request);
            if (checkResult.ErrorValue.Item1 != 0)
            {
                return checkResult;
            }
            // 获取当前工单
            var deviceInfo = await _dbContext.DeviceInfos
                .FirstOrDefaultAsync(x => x.Resource == request.Resource);
            if (deviceInfo == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource}不存在"));
            }

            var workOrder = await _dbContext.OrderList
                .FirstOrDefaultAsync(x => x.OrderCode == deviceInfo.WorkOrderCode);
            if (workOrder == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单{deviceInfo.WorkOrderCode}不存在"));
            }
            if (checkResult.ErrorValue.Item2 == "首站")
            {
            

            // 获取首站站点
            var firstStation = await (from p in _dbContext.ProcessRoutes
                                      join i in _dbContext.ProcessRouteItems on p.Id equals i.HeadId
                                      join s in _dbContext.StationList on i.StationCode equals s.StationCode
                                      where p.Id == workOrder.ProcessRouteId && i.FirstStation == true
                                      select new { s.StationId, s.StationCode })
                                     .FirstOrDefaultAsync();

            if (firstStation == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "工艺路线未配置首站"));
            }

            // 1. 先往 mesSnListCurrents 插入数据
            var insertSnCurrent = new MesSnListCurrent
            {
                SnNumber = request.SN,
                OrderListId = workOrder.OrderListId,
                CurrentStationListId = firstStation.StationId,
                ResourceId = deviceInfo.ResourceId,
                ProductionLineId = deviceInfo.ProductionLineId,
                StationStatus = 1,      // PASS
                IsAbnormal = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            await _dbContext.mesSnListCurrents.AddAsync(insertSnCurrent);
            await _dbContext.SaveChangesAsync();

            // 2. 再往 mesSnListHistories 插入数据
            var InsertSnHistory = new MesSnListHistory
            {
                SnNumber = insertSnCurrent.SnNumber,
                OrderListId = insertSnCurrent.OrderListId,
                CurrentStationListId = insertSnCurrent.CurrentStationListId,
                StationStatus = 1,
                ResourceId = deviceInfo.ResourceId,
                ProductionLineId = deviceInfo.ProductionLineId,
                IsAbnormal = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            await _dbContext.mesSnListHistories.AddAsync(InsertSnHistory);
            await _dbContext.SaveChangesAsync();

            return FSharpResult<ValueTuple, (int, string)>.NewOk(default(ValueTuple));

            }
            // 获取当前SN在mesSnListCurrents中的记录
            var snCurrent = await _dbContext.mesSnListCurrents
                .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
            if (snCurrent == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
            }

            // 获取上传站点信息
            var station = await _dbContext.StationList
                .FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
            if (station == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode}不存在"));
            }

            // 将当前记录写入历史表
            var snHistory = new MesSnListHistory
            {
                SnNumber = snCurrent.SnNumber,
                OrderListId = snCurrent.OrderListId,
                CurrentStationListId = snCurrent.CurrentStationListId,
                ProductionLineId = deviceInfo.ProductionLineId,
                ResourceId = deviceInfo.ResourceId,
                StationStatus = 1,                 // 默认PASS
                IsAbnormal = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                TestData=request.TestData,
            };
            await _dbContext.mesSnListHistories.AddAsync(snHistory);

            // 更新Current表：移动到下一站点
            var processRouteItems = await (from p in _dbContext.ProcessRoutes
                                     join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                     join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                     join ord in _dbContext.OrderList on p.Id equals ord.ProcessRouteId
                                     where ord.OrderListId == snCurrent.OrderListId
                                     orderby iitm.RouteSeq
                                     select new { iitm.StationCode, s.StationId, iitm.RouteSeq })
                                    .ToListAsync();

            var currentStep = processRouteItems.FirstOrDefault(x => x.StationCode == request.StationCode);
            if (currentStep == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线中找不到站点{request.StationCode}"));
            }

            var nextStep = processRouteItems
                .Where(x => x.RouteSeq > currentStep.RouteSeq)
                .OrderBy(x => x.RouteSeq)
                .FirstOrDefault();

            if (nextStep == null)
            {
                // 最后一站，标记完工
                //snCurrent.CurrentStationListId = null;
                snCurrent.StationStatus = 4;
            }
            else
            {
                snCurrent.CurrentStationListId = nextStep.StationId;
            }

            snCurrent.UpdateTime = DateTime.Now;
            _dbContext.mesSnListCurrents.Update(snCurrent);
            await _dbContext.SaveChangesAsync();
            

            return FSharpResult<ValueTuple, (int, string)>.NewOk(default(ValueTuple));
        }

         
    }
}