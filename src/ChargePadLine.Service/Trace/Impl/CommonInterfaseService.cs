using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.BOM;
using ChargePadLine.Entitys.Trace.Order;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.FSharp.Core.ByRefKinds;
using System.Text.RegularExpressions;
using ChargePadLine.Service.Trace.Dto.BOM;

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
        private readonly IRepository<MesSnListHistory> _mesSNListHistory;
        private readonly ILogger<OrderListService> _logger;
        private readonly AppDbContext _dbContext;

        public CommonInterfaseService(
            IRepository<WorkOrder> workOrderRepo,
            IRepository<BomRecipe> bomRecipeRepo,
            IRepository<Material> materialRepo,
            IRepository<MesSnListHistory> mesSNListHistory,
            ILogger<OrderListService> logger,
            AppDbContext dbContext)
        {
            _workOrderRepo = workOrderRepo;
            _bomRecipeRepo = bomRecipeRepo;
            _materialRepo = materialRepo;
            _logger = logger;
            _dbContext = dbContext;
            _mesSNListHistory = mesSNListHistory;
        }

        public Task<string> GetName()
        {
            return null;
        }

        public Task<List<string>> GetList()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 投料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<FSharpResult<ValueTuple, (int, string)>> FeedMaterial(RequestFeedMaterialParams request)
        {
            var batchQty = 0;
            Guid ResourceId;
            if (request.Resource.IsNullOrEmpty())
            {
               return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
            }
            else
            {
                var DeviceInfosList =  _dbContext.DeviceInfos.FirstOrDefault(x => x.Resource == request.Resource);
                if (DeviceInfosList == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
                }
                if (DeviceInfosList.WorkOrderCode == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
                }

                // 3. 检查工单是否存在且状态正确
                var workOrder = _dbContext.OrderList.FirstOrDefault(x => x.OrderCode == DeviceInfosList.WorkOrderCode);
                if (workOrder == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{DeviceInfosList.WorkOrderCode}"));
                }
                request.WorkOrderCode = DeviceInfosList.WorkOrderCode;
                ResourceId = DeviceInfosList.ResourceId;
            }

            if (request.ProductCode.IsNullOrEmpty())
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"产品信息不能为空：{request.ProductCode}"));
            }

            var product = _dbContext.ProductList.FirstOrDefault(x => x.ProductCode == request.ProductCode);

            if (product == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"产品不存在，产品编号：{request.ProductCode}"));
            }

            if (request.StationCode.IsNullOrEmpty())
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点信息不能为空：{request.StationCode}"));
            }
            // 4. 检查上传站点是否存在
            var StationList = _dbContext.StationList.FirstOrDefault(x => x.StationCode == request.StationCode);
            if (StationList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
            }

            var Order = _dbContext.OrderList.FirstOrDefault(x => x.OrderCode == request.WorkOrderCode);

            if (Order == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{request.WorkOrderCode}"));
            }
            var BomId = Order?.BomId;

            var BomItemList = _dbContext.BomItem.FirstOrDefault(x => x.BomId == BomId && x.StationCode == request.StationCode && x.ProductId == product.ProductListId);

            if (BomItemList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"BOM不存在，BOM编号：{BomId}"));
            }

            if (request.BatchCode.IsNullOrEmpty() || request.BatchCode == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次信息不能为空：{request.BatchCode}"));
            }

            // 正则表达式（带分组）
            string pattern = BomItemList.BatchRule;

            // 创建 Regex 对象
            Regex regex = new Regex(pattern);

            // 匹配
            //Match match = regex.Match(request.BatchCode);
            Match match = Regex.Match(request.BatchCode, pattern);

            if(pattern=="%" && request.BatchQty > 0)
            {
                batchQty = request.BatchQty;
            }
            else
            {
                if (match.Success)
                {
                    //Console.WriteLine("匹配成功！");
                    //Console.WriteLine($"完整匹配: {match.Value}");
                    //Console.WriteLine($"日期: {match.Groups[1].Value}");  // 第一组：日期
                    //Console.WriteLine($"数量: {match.Groups[2].Value}");  // 第二组：数量
                    //Console.WriteLine($"流水号: {match.Groups[3].Value}"); // 第三组：流水号



                    if (product.ProductCode != match.Groups[1].Value)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次规则错误，批次物料{match.Groups[1].Value}，BOM物料{product.ProductCode}"));
                    }

                    var BomItemQty = 0;
                    if (request.BatchQty > 0)
                    {
                        BomItemQty = Convert.ToInt32(request.BatchQty);
                    }
                    else
                    {
                        if (BomItemList.BatchQty == true)
                        {
                            BomItemQty = Convert.ToInt32(BomItemList.BatchSNQty);
                        }
                        else
                        {
                            var splitBatchSNQty = BomItemList.BatchSNQty.Split(",");
                            BomItemQty = Convert.ToInt32(request.BatchCode.Substring(Convert.ToInt32(splitBatchSNQty[0]), Convert.ToInt32(splitBatchSNQty[1])));
                        }
                    }
                    batchQty = BomItemQty;
                }
                else
                {

                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次规则错误"));
                }
            }

            

            MesOrderBomBatch mesOrderBomBatch = new MesOrderBomBatch();
            mesOrderBomBatch.OrderBomBatchId = Guid.NewGuid();
            mesOrderBomBatch.OrderListId = Order.OrderListId;
            mesOrderBomBatch.ProductListId = product.ProductListId;
            mesOrderBomBatch.BatchCode = request.BatchCode;
            mesOrderBomBatch.BatchQty = batchQty;
            mesOrderBomBatch.StationListId = StationList.StationId;
            mesOrderBomBatch.CreateTime = DateTimeOffset.Now;
            mesOrderBomBatch.UpdateTime = DateTimeOffset.Now;
            mesOrderBomBatch.OrderBomBatchStatus = 1;
            mesOrderBomBatch.CompletedQty = 0;
            mesOrderBomBatch.ResourceId = ResourceId;
            _dbContext.MesOrderBomBatch.Add(mesOrderBomBatch);
            _dbContext.SaveChanges();



            return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"上料成功"));
        }

        /// <summary>
        /// 上传前校验：检查设备、工单、工艺路线、SN 状态等
        /// </summary>
        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadCheck(RequestUploadCheckParams request)
        {
            if (request.Resource.IsNullOrEmpty() && request.WorkOrderCode.IsNullOrEmpty())
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码和工单不能同时为空"));
            }
            var DeviceInfosList = new Deviceinfo();
            var workOrder=new OrderList();
            if (!request.Resource.IsNullOrEmpty())
            {
                // 1. 根据资源号获取设备信息
                DeviceInfosList = _dbContext.DeviceInfos.FirstOrDefault(x => x.Resource == request.Resource);
                if (DeviceInfosList == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
                }

                // 2. 检查设备是否已绑定工单
                if (DeviceInfosList.WorkOrderCode == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
                }
                request.WorkOrderCode = DeviceInfosList.WorkOrderCode;
            }
            //手工站根据SN 获取工单
            else if (request.WorkOrderCode.IsNullOrEmpty())
            {
                var FirstSNList = _dbContext.mesSnListCurrents.First(x => x.SnNumber == request.SN);
                workOrder = _dbContext.OrderList.FirstOrDefault(x => x.OrderListId == FirstSNList.OrderListId);
                if (workOrder == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，SN：{request.SN}"));
                }
                request.WorkOrderCode = workOrder.OrderCode;
            }




            // 3. 检查工单是否存在且状态正确
            workOrder = _dbContext.OrderList.FirstOrDefault(x => x.OrderCode == request.WorkOrderCode);
            if (workOrder == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{request.WorkOrderCode}"));
            }

            // 4. 检查上传站点是否存在
            var StationList = _dbContext.StationList.FirstOrDefault(x => x.StationCode == request.StationCode);
            if (StationList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
            }

            // 5. 取出该工单对应工艺路线下的所有站点明细
            var processRouteItems = from p in _dbContext.ProcessRoutes
                                    join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                    join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                    where p.Id == workOrder.ProcessRouteId
                                    orderby iitm.RouteSeq
                                    select new { p.RouteCode, p.RouteName, iitm.FirstStation, iitm.MustPassStation, iitm.RouteSeq, iitm.StationCode, iitm.CheckAll, iitm.CheckStationList, s.StationId };

            // 6. 工艺路线为空则直接返回错误
            if (processRouteItems.Count() == 0)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线不存在，工单编号：{request.WorkOrderCode}"));
            }

            // 7. 找到当前上传站点在工艺路线中的记录
            var currentprocessRouteList = processRouteItems.Where(x => x.StationCode == request.StationCode).First();

            // 8. 如果是首站，检查 SN 是否已存在
            if (currentprocessRouteList.FirstStation == true)
            {
                var snCurrent = await _dbContext.mesSnListCurrents
               .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (snCurrent != null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"首站站点SN已存在{request.SN}"));
                }

            }
            else
            {
                // 9. 非首站：检查 SN 是否存在
                var SNList = _dbContext.mesSnListCurrents.FirstOrDefault(x => x.SnNumber == request.SN);
                if (SNList == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},不存在"));
                }

                // 10. 检查工单状态是否为生产中
                if (workOrder.OrderStatus != 3)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单状态不是生产中，工单状态：{workOrder.OrderStatus.ToString()}"));
                }

                // 11. 检查 SN 当前站点与上传站点是否一致
                if (SNList.CurrentStationListId != StationList.StationId)
                {
                    var SNStationList = _dbContext.StationList.FirstOrDefault(x => x.StationId == SNList.CurrentStationListId);
                    if (SNStationList == null)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不存在"));
                    }
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不一致，SN站点{SNStationList.StationCode}，上传站点{request.StationCode}"));
                }

                // 12. 检查 SN 是否异常
                if (SNList.IsAbnormal == true)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前状态异常"));
                }

                // 13. 取出 SN 历史过站记录
                var SnListhistory = from snhis in _dbContext.mesSnListHistories
                                    join station in _dbContext.StationList on snhis.CurrentStationListId equals station.StationId
                                    where snhis.SnNumber == request.SN
                                    select new
                                    {
                                        SnHistory = snhis,      // 包含mesSnListHistories表的所有列
                                        Station = station       // 包含StationList表的所有列
                                    };

                // 14. 如果当前站点要求检查所有站点都PASS
                if (currentprocessRouteList.CheckAll == true)
                {
                    foreach (var processRouteItem in processRouteItems)
                    {
                        if (SnListhistory.Where(x => x.SnHistory.CurrentStationListId == processRouteItem.StationId && x.SnHistory.StationStatus == 1).Count() == 0)
                        {
                            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{processRouteItem.StationCode}没有PASS记录"));
                        }
                    }
                }

                // 15. 如果当前站点配置了必须检查的站点列表
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
                    // 16. 否则只检查上一必过站是否有PASS记录
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

            var orderBom = await (from ord in _dbContext.OrderList
                                  join bomitem in _dbContext.BomItem on ord.BomId equals bomitem.BomId
                                  join sta in _dbContext.StationList on bomitem.StationCode equals sta.StationCode
                                  where ord.OrderListId == workOrder.OrderListId && bomitem.StationCode == request.StationCode
                                  select new
                                  {
                                      OrderBom = ord,
                                      BomItem = bomitem,
                                      Station = sta
                                  }).ToListAsync();

            foreach (var orderBomItem in orderBom)
            {
                // 17. 检查MesOrderBomBatch上料情况
                var orderBomBatch = _dbContext.MesOrderBomBatch
                    .FirstOrDefault(x => x.OrderListId == workOrder.OrderListId && x.StationListId == orderBomItem.Station.StationId && x.ProductListId == orderBomItem.BomItem.ProductId);
                if (orderBomBatch == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}未进行上料操作"));
                }

                // 18. 检查上料状态是否正常
                if (orderBomBatch.OrderBomBatchStatus != 1)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料状态异常，状态：{orderBomBatch.OrderBomBatchStatus}"));
                }

                // 19. 检查上料数量是否足够（已使用数量 < 批次数量）
                if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料批次已使用完"));
                }
            }

            if (currentprocessRouteList.FirstStation == true)
            {
                // 首站不继续后续校验，直接返回
                return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"首站"));
            }
            // 20. 所有校验通过
            return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"检查通过"));
        }
        /// <summary>
        /// 数据上传主流程：
        /// 1. 复用 UploadCheck 做全部前置校验；
        /// 2. 首站：向 mesSnListCurrents 插入新记录，更新工单状态；
        /// 3. 非首站：将当前站点记录写入历史表，测试 PASS 时自动跳站，NG 则停留；
        /// 4. 最后一站：将 Current 表状态置为完工(4)。
        /// </summary>
        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadData(RequestUploadCheckParams request)
        {
            // ---------- 1. 通用校验 ----------
            var checkResult = await UploadCheck(request);
            if (checkResult.ErrorValue.Item1 != 0)
            {
                return checkResult;   // 校验不通过直接返回
            }

            // ---------- 2. 提取常用数据 ----------
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

            // 测试结果转换：PASS=1，其它=2
            var uploadStationStatus = request.TestResult == "PASS" ? 1 : 2;

            // ---------- 3. 首站逻辑 ----------
            if (checkResult.ErrorValue.Item2 == "首站")
            {
                // 再次防重
                var snCurrent = await _dbContext.mesSnListCurrents
                    .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (snCurrent != null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "首站站点SN已存在"));
                }

                // 取首站站点
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

                // 写入 Current 表
                var insertSnCurrent = new MesSnListCurrent
                {
                    SnNumber = request.SN,
                    OrderListId = workOrder.OrderListId,
                    CurrentStationListId = firstStation.StationId,
                    ResourceId = deviceInfo.ResourceId,
                    ProductListId = workOrder.ProductListId,
                    ProductionLineId = deviceInfo.ProductionLineId,
                    StationStatus = uploadStationStatus,
                    IsAbnormal = false,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                await _dbContext.mesSnListCurrents.AddAsync(insertSnCurrent);

                // 更新工单：状态=生产中(3)，累计完成数+1，写入实际开始时间
                workOrder.OrderStatus = 3;
                workOrder.CompletedQty += 1;
                workOrder.ActualStartTime = DateTime.Now;
                _dbContext.OrderList.Update(workOrder);

                await _dbContext.SaveChangesAsync();
                // 首站到此结束，不再走后续跳站逻辑
            }

            {
                // ---------- 4. 非首站：写历史 + 跳站 ----------
                var station = await _dbContext.StationList
                    .FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
                if (station == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode}不存在"));
                }

                var snCurrent = await _dbContext.mesSnListCurrents
                    .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (snCurrent == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
                }

                // 4.1 写入历史表
                var snHistory = new MesSnListHistory
                {
                    SnNumber = snCurrent.SnNumber,
                    OrderListId = snCurrent.OrderListId,
                    CurrentStationListId = snCurrent.CurrentStationListId,
                    ProductionLineId = deviceInfo.ProductionLineId,
                    ProductListId = workOrder.ProductListId,
                    ResourceId = deviceInfo.ResourceId,
                    StationStatus = uploadStationStatus,
                    IsAbnormal = false,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    TestData = request.TestData
                };
                await _mesSNListHistory.InsertAsync(snHistory);   // 使用仓储插入

                // 4.2 测试 NG 直接返回，不再跳站
                if (uploadStationStatus != 1)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"测试结果为{request.TestResult}，数据上传成功{request.SN}"));
                }


                var orderBom = await (from ord in _dbContext.OrderList
                                      join bomitem in _dbContext.BomItem on ord.BomId equals bomitem.BomId
                                      join sta in _dbContext.StationList on bomitem.StationCode equals sta.StationCode
                                      where ord.OrderListId == workOrder.OrderListId && bomitem.StationCode == request.StationCode
                                      select new
                                      {
                                          OrderBom = ord,
                                          BomItem = bomitem,
                                          Station = sta
                                      }).ToListAsync();

                foreach (var orderBomItem in orderBom)
                {
                    // 17. 检查MesOrderBomBatch上料情况
                    var orderBomBatch = _dbContext.MesOrderBomBatch
                        .FirstOrDefault(x => x.OrderListId == workOrder.OrderListId && x.StationListId == orderBomItem.Station.StationId && x.ProductListId == orderBomItem.BomItem.ProductId);
                    if (orderBomBatch == null)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}未进行上料操作"));
                    }

                    // 18. 检查上料状态是否正常
                    if (orderBomBatch.OrderBomBatchStatus != 1)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料状态异常，状态：{orderBomBatch.OrderBomBatchStatus}"));
                    }

                    // 19. 检查上料数量是否足够（已使用数量 < 批次数量）
                    if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料批次已使用完"));
                    }
                    orderBomBatch.CompletedQty += 1;
                    if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
                    {
                        orderBomBatch.OrderBomBatchStatus = 2;
                    }

                    var orderBomBatchitem = new MesOrderBomBatchItem
                    {
                        OrderBomBatchId = orderBomBatch.OrderBomBatchId,
                        SnNumber = request.SN,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    };

                    await _dbContext.MesOrderBomBatchItem.AddAsync(orderBomBatchitem);
                    await _dbContext.SaveChangesAsync();

                    _dbContext.MesOrderBomBatch.Update(orderBomBatch);
                    await _dbContext.SaveChangesAsync();
                }

                // 4.3 取工艺路线并计算下一站点
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
                    // 最后一站：状态置为完工(4)
                    snCurrent.StationStatus = 4;
                }
                else
                {
                    // 跳到下一站
                    snCurrent.CurrentStationListId = nextStep.StationId;
                    snCurrent.ResourceId = deviceInfo.ResourceId;
                    snCurrent.ProductionLineId = deviceInfo.ProductionLineId;
                }

                snCurrent.UpdateTime = DateTime.Now;
                _dbContext.mesSnListCurrents.Update(snCurrent);
                await _dbContext.SaveChangesAsync();
            }

            // ---------- 5. 全部完成 ----------
            return FSharpResult<ValueTuple, (int, string)>.NewOk(default(ValueTuple));
        }

        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadData1(RequestUploadCheckParams request)
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

            var uploadStationStatus = request.TestResult == "PASS" ? 1 : 2;
            var snCurrent = await _dbContext.mesSnListCurrents
                .FirstOrDefaultAsync(x => x.SnNumber == request.SN);

            if (checkResult.ErrorValue.Item2 == "首站")
            {
                if (snCurrent != null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "首站站点SN已存在"));
                }
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
                    StationStatus = uploadStationStatus,      // PASS
                    IsAbnormal = false,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                await _dbContext.mesSnListCurrents.AddAsync(insertSnCurrent);
                await _dbContext.SaveChangesAsync();

                workOrder.OrderStatus = 3;
                workOrder.CompletedQty = workOrder.CompletedQty + 1;
                workOrder.ActualStartTime = DateTime.Now;
                _dbContext.OrderList.Update(workOrder);
                await _dbContext.SaveChangesAsync();


                // 2. 再往 mesSnListHistories 插入数据
                //var InsertSnHistory = new MesSnListHistory
                //{
                //    SnNumber = insertSnCurrent.SnNumber,
                //    OrderListId = insertSnCurrent.OrderListId,
                //    CurrentStationListId = insertSnCurrent.CurrentStationListId,
                //    StationStatus = uploadStationStatus,
                //    ResourceId = deviceInfo.ResourceId,
                //    ProductionLineId = deviceInfo.ProductionLineId,
                //    IsAbnormal = false,
                //    CreateTime = DateTime.Now,
                //    UpdateTime = DateTime.Now
                //};
                //await _dbContext.mesSnListHistories.AddAsync(InsertSnHistory);
                //await _dbContext.SaveChangesAsync();

                // return FSharpResult<ValueTuple, (int, string)>.NewOk(default(ValueTuple));

            }
            // 获取当前SN在mesSnListCurrents中的记录



            // 获取上传站点信息
            var station = await _dbContext.StationList
                .FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
            if (station == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode}不存在"));
            }
            snCurrent = await _dbContext.mesSnListCurrents
                .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
            if (snCurrent == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
            }
            // 将当前记录写入历史表
            var snHistory = new MesSnListHistory
            {
                SnNumber = snCurrent.SnNumber,
                OrderListId = snCurrent.OrderListId,
                CurrentStationListId = snCurrent.CurrentStationListId,
                ProductionLineId = deviceInfo.ProductionLineId,
                ProductListId = workOrder.ProductListId,
                ResourceId = deviceInfo.ResourceId,
                StationStatus = uploadStationStatus,                 // 默认PASS
                IsAbnormal = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                TestData = request.TestData,
            };
            await _mesSNListHistory.InsertAsync(snHistory);
            //await _dbContext.mesSnListHistories.AddAsync(snHistory);
            //await _dbContext.SaveChangesAsync();

            // 测试结果为NG，的不跳站
            if (uploadStationStatus != 1)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"测试结果为{request.TestResult}，数据上传成功{request.SN}"));
            }

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
            //获取下一站点
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
        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadCheck1(RequestUploadCheckParams request)
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



            var StationList = _dbContext.StationList.FirstOrDefault(x => x.StationCode == request.StationCode);
            if (StationList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
            }

            var processRouteItems = from p in _dbContext.ProcessRoutes
                                    join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                    join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                    where p.Id == workOrder.ProcessRouteId
                                    orderby iitm.RouteSeq
                                    select new { p.RouteCode, p.RouteName, iitm.FirstStation, iitm.MustPassStation, iitm.RouteSeq, iitm.StationCode, iitm.CheckAll, iitm.CheckStationList, s.StationId };
            //  select new { p.RouteCode,iitm. };

            if (processRouteItems.Count() == 0)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线不存在，工单编号：{DeviceInfosList.WorkOrderCode}"));
            }
            var currentprocessRouteList = processRouteItems.Where(x => x.StationCode == request.StationCode).First();
            /// 判断是否是第一站
            if (currentprocessRouteList.FirstStation == true)
            {
                var snCurrent = await _dbContext.mesSnListCurrents
               .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (snCurrent != null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"首站站点SN已存在{request.SN}"));
                }
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
                        if (SnListhistory.Where(x => x.SnHistory.CurrentStationListId == processRouteItem.StationId && x.SnHistory.StationStatus == 1).Count() == 0)
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



    }
}