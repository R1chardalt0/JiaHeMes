using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.QualityManagement;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
    /// <summary>
    /// 生产报告服务实现类
    /// 提供生产数据统计、质量分析等报告生成功能
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly AppDbContext _dbContext;
        private readonly IRepository<MesSnListHistory> _mesSnListHistoryRepo;
        private readonly IRepository<MesSnListCurrent> _mesSnListCurrentRepo;
        private readonly IRepository<MesSnTestData> _mesSnTestDataRepo;
        private readonly IRepository<StationList> _stationListRepo;
        private readonly IRepository<ProductionLine> _productionLineRepo;
        private readonly IRepository<DefectRecord> _defectRecordRepo;
        private readonly ILogger<ReportService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="mesSnListHistoryRepo">SN历史记录仓储</param>
        /// <param name="mesSnListCurrentRepo">SN当前状态仓储</param>
        /// <param name="mesSnTestDataRepo">SN测试数据仓储</param>
        /// <param name="stationListRepo">站点信息仓储</param>
        /// <param name="productionLineRepo">生产线仓储</param>
        /// <param name="defectRecordRepo">缺陷记录仓储</param>
        /// <param name="logger">日志记录器</param>
        public ReportService(
            AppDbContext dbContext,
            IRepository<MesSnListHistory> mesSnListHistoryRepo,
            IRepository<MesSnListCurrent> mesSnListCurrentRepo,
            IRepository<MesSnTestData> mesSnTestDataRepo,
            IRepository<StationList> stationListRepo,
            IRepository<ProductionLine> productionLineRepo,
            IRepository<DefectRecord> defectRecordRepo,
            ILogger<ReportService> logger)
        {
            _dbContext = dbContext;
            _mesSnListHistoryRepo = mesSnListHistoryRepo;
            _mesSnListCurrentRepo = mesSnListCurrentRepo;
            _mesSnTestDataRepo = mesSnTestDataRepo;
            _stationListRepo = stationListRepo;
            _productionLineRepo = productionLineRepo;
            _defectRecordRepo = defectRecordRepo;
            _logger = logger;
        }

        /// <summary>
        /// 获取当日每小时产出统计
        /// </summary>
        /// <param name="productionLineId">生产线ID（可选）</param>
        /// <param name="workOrderId">工单ID（可选）</param>
        /// <returns>每小时产出统计数据</returns>
        public async Task<List<HourlyOutputDto>> GetHourlyOutputAsync(Guid? productionLineId = null, Guid? workOrderId = null)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // 查询当日所有生产记录
            var query = _dbContext.mesSnListHistories
                .Where(h => h.CreateTime >= today && h.CreateTime < tomorrow);

            // 添加生产线过滤
            if (productionLineId.HasValue)
            {
                query = query.Where(h => h.ProductionLineId == productionLineId.Value);
            }

            // 添加工单过滤
            if (workOrderId.HasValue)
            {
                query = query.Where(h => h.OrderListId == workOrderId.Value);
            }

            // 按小时分组统计
            var hourlyStats = await query
                .GroupBy(h => h.CreateTime.Value.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TotalCount = g.Count(),
                    PassCount = g.Count(h => h.StationStatus == 1), // 状态1表示合格
                    FailCount = g.Count(h => h.StationStatus != 1) // 其他状态表示不合格
                })
                .ToListAsync();

            // 构建完整的24小时数据
            var result = new List<HourlyOutputDto>();
            for (int hour = 0; hour < 24; hour++)
            {
                var stat = hourlyStats.FirstOrDefault(s => s.Hour == hour);
                var hourStartTime = today.AddHours(hour);
                var hourEndTime = hourStartTime.AddHours(1);

                result.Add(new HourlyOutputDto
                {
                    Hour = hour,
                    OutputQuantity = stat?.TotalCount ?? 0,
                    PassQuantity = stat?.PassCount ?? 0,
                    FailQuantity = stat?.FailCount ?? 0,
                    HourStartTime = hourStartTime,
                    HourEndTime = hourEndTime
                });
            }

            return result;
        }

        /// <summary>
        /// 计算OEE（设备综合效率）
        /// OEE = 可用性 × 性能效率 × 质量合格率
        /// </summary>
        /// <param name="request">OEE计算请求参数</param>
        /// <returns>OEE计算结果</returns>
        public async Task<OeeCalculationResultDto> CalculateOEEAsync(OeeCalculationRequestDto request)
        {
            // 获取实际产出数据
            var actualOutputQuery = _dbContext.mesSnListHistories
                .Where(h => h.ResourceId == request.ResourceId &&
                           h.CreateTime >= request.StartTime &&
                           h.CreateTime <= request.EndTime);

            var actualOutput = await actualOutputQuery.CountAsync();
            var goodOutput = await actualOutputQuery.CountAsync(h => h.StationStatus == 1);

            // 如果请求中提供了实际产出数据，则使用请求中的数据
            if (request.ActualOutput.HasValue)
            {
                actualOutput = request.ActualOutput.Value;
            }

            if (request.GoodOutput.HasValue)
            {
                goodOutput = request.GoodOutput.Value;
            }

            // 计算实际运行时间（如果请求中未提供）
            double actualRunTime = request.ActualRunTime ?? request.PlannedProductionTime;

            // 计算理论产出
            int theoreticalOutput = (int)(actualRunTime * 60 / request.TheoreticalCycleTime);

            // 计算可用性 = 实际运行时间 / 计划生产时间
            double availability = actualRunTime / request.PlannedProductionTime;

            // 计算性能效率 = 实际产出 / 理论产出
            double performanceEfficiency = theoreticalOutput > 0 ? (double)actualOutput / theoreticalOutput : 0;

            // 计算质量合格率 = 合格产出 / 实际产出
            double qualityRate = actualOutput > 0 ? (double)goodOutput / actualOutput : 0;

            // 计算OEE
            double oeeValue = availability * performanceEfficiency * qualityRate;

            // 计算停机时间
            double downtime = request.PlannedProductionTime - actualRunTime;

            return new OeeCalculationResultDto
            {
                Availability = availability,
                PerformanceEfficiency = performanceEfficiency,
                QualityRate = qualityRate,
                OeeValue = oeeValue,
                TheoreticalOutput = theoreticalOutput,
                ActualOutput = actualOutput,
                GoodOutput = goodOutput,
                PlannedProductionTime = request.PlannedProductionTime,
                ActualRunTime = actualRunTime,
                Downtime = downtime
            };
        }

        /// <summary>
        /// 获取各站NG件统计
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="productionLineId">生产线ID（可选）</param>
        /// <returns>各站NG件统计数据</returns>
        public async Task<List<StationNGStatisticsDto>> GetStationNGStatisticsAsync(DateTime startTime, DateTime endTime, Guid? productionLineId = null)
        {
            // 查询指定时间段内的所有不合格记录
            var query = _dbContext.mesSnListHistories
                .Include(h => h.StationList)
                .Where(h => h.CreateTime >= startTime && h.CreateTime <= endTime && h.StationStatus != 1);

            // 添加生产线过滤
            if (productionLineId.HasValue)
            {
                query = query.Where(h => h.ProductionLineId == productionLineId.Value);
            }

            // 按站点分组统计NG数量
            var stationStats = await query
                .GroupBy(h => new { h.StationList.StationId, h.StationList.StationCode, h.StationList.StationName })
                .Select(g => new
                {
                    StationId = g.Key.StationId,
                    StationCode = g.Key.StationCode,
                    StationName = g.Key.StationName,
                    NGCount = g.Count()
                })
                .ToListAsync();

            // 查询各站点的总测试数量
            var totalTestQuery = _dbContext.mesSnListHistories
                .Include(h => h.StationList)
                .Where(h => h.CreateTime >= startTime && h.CreateTime <= endTime);

            if (productionLineId.HasValue)
            {
                totalTestQuery = totalTestQuery.Where(h => h.ProductionLineId == productionLineId.Value);
            }

            var totalTestStats = await totalTestQuery
                .GroupBy(h => h.StationList.StationId)
                .Select(g => new
                {
                    StationId = g.Key,
                    TotalTestCount = g.Count()
                })
                .ToDictionaryAsync(g => g.StationId, g => g.TotalTestCount);

            // 构建结果
            var result = stationStats.Select(stat =>
            {
                int totalTestCount = totalTestStats.TryGetValue(stat.StationId, out var count) ? count : 0;
                double ngRate = totalTestCount > 0 ? (double)stat.NGCount / totalTestCount : 0;

                return new StationNGStatisticsDto
                {
                    StationId = stat.StationId,
                    StationCode = stat.StationCode,
                    StationName = stat.StationName,
                    NGCount = stat.NGCount,
                    TotalTestCount = totalTestCount,
                    NGRate = ngRate,
                    DefectDetails = new List<DefectDetailDto>() // 缺陷明细暂时为空，可根据实际需求扩展
                };
            }).ToList();

            return result;
        }

        /// <summary>
        /// 计算一次通过率
        /// 一次通过率 = 一次通过数量 / 总投入数量
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="workOrderId">工单ID（可选）</param>
        /// <returns>一次通过率结果</returns>
        public async Task<FirstPassYieldDto> CalculateFirstPassYieldAsync(DateTime startTime, DateTime endTime, Guid? workOrderId = null)
        {
            // 查询指定时间段内的所有SN
            var snQuery = _dbContext.mesSnListHistories
                .Where(h => h.CreateTime >= startTime && h.CreateTime <= endTime)
                .Select(h => h.SnNumber)
                .Distinct();

            // 添加工单过滤
            if (workOrderId.HasValue)
            {
                snQuery = snQuery.Where(sn => _dbContext.mesSnListHistories.Any(h => h.SnNumber == sn && h.OrderListId == workOrderId.Value));
            }

            var allSns = await snQuery.ToListAsync();
            int totalInput = allSns.Count;

            // 计算一次通过的SN数量
            int firstPassCount = 0;

            foreach (var sn in allSns)
            {
                // 查询该SN的所有生产记录
                var snHistories = await _dbContext.mesSnListHistories
                    .Where(h => h.SnNumber == sn && h.CreateTime >= startTime && h.CreateTime <= endTime)
                    .OrderBy(h => h.CreateTime)
                    .ToListAsync();

                // 如果该SN没有失败记录，则视为一次通过
                if (snHistories.All(h => h.StationStatus == 1))
                {
                    firstPassCount++;
                }
            }

            // 计算一次通过率
            double firstPassYield = totalInput > 0 ? (double)firstPassCount / totalInput : 0;

            return new FirstPassYieldDto
            {
                StartTime = startTime,
                EndTime = endTime,
                TotalInput = totalInput,
                FirstPassCount = firstPassCount,
                FirstPassYield = firstPassYield
            };
        }

        /// <summary>
        /// 计算合格率/不良率
        /// 合格率 = 合格数量 / 总测试数量
        /// 不良率 = 不良数量 / 总测试数量
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="productionLineId">生产线ID（可选）</param>
        /// <returns>合格率/不良率结果</returns>
        public async Task<QualityRateDto> CalculateQualityRateAsync(DateTime startTime, DateTime endTime, Guid? productionLineId = null)
        {
            // 查询指定时间段内的所有测试记录
            var query = _dbContext.mesSnListHistories
                .Where(h => h.CreateTime >= startTime && h.CreateTime <= endTime);

            // 添加生产线过滤
            if (productionLineId.HasValue)
            {
                query = query.Where(h => h.ProductionLineId == productionLineId.Value);
            }

            // 统计总测试数量、合格数量和不良数量
            var stats = await query
                .Select(h => new { h.StationStatus })
                .ToListAsync();

            int totalTestCount = stats.Count;
            int passCount = stats.Count(s => s.StationStatus == 1);
            int failCount = totalTestCount - passCount;

            // 计算合格率和不良率
            double passRate = totalTestCount > 0 ? (double)passCount / totalTestCount : 0;
            double failRate = totalTestCount > 0 ? (double)failCount / totalTestCount : 0;

            return new QualityRateDto
            {
                StartTime = startTime,
                EndTime = endTime,
                TotalTestCount = totalTestCount,
                PassCount = passCount,
                FailCount = failCount,
                PassRate = passRate,
                FailRate = failRate
            };
        }

        /// <summary>
        /// 计算过程能力指数
        /// Cp = (规格上限 - 规格下限) / (6 × 标准差)
        /// Cpk = min(Cpu, Cpl)
        /// Cpu = (规格上限 - 均值) / (3 × 标准差)
        /// Cpl = (均值 - 规格下限) / (3 × 标准差)
        /// Ca = |均值 - 目标值| / [(规格上限 - 规格下限) / 2]
        /// </summary>
        /// <param name="request">过程能力指数计算请求参数</param>
        /// <returns>过程能力指数计算结果</returns>
        public async Task<ProcessCapabilityDto> CalculateProcessCapabilityAsync(ProcessCapabilityRequestDto request)
        {
            // 首先查询符合条件的历史记录ID
            var historyIds = await _dbContext.mesSnListHistories
                .Where(h => h.CreateTime >= request.StartTime &&
                           h.CreateTime <= request.EndTime &&
                           h.StationList.StationId == request.StationId)
                .Select(h => h.SNListHistoryId)
                .ToListAsync();

            // 然后查询对应的测试数据
            var testDataQuery = _dbContext.mesSnTestDatas
                .Where(t => historyIds.Contains(t.SNListHistoryId) &&
                           t.ParametricKey == request.ParametricKey);

            // 提取测试值（转换为double）
            var testValues = await testDataQuery
                .Where(t => !string.IsNullOrWhiteSpace(t.TestValue))
                .Select(t => Convert.ToDouble(t.TestValue))
                .ToListAsync();

            int sampleSize = testValues.Count;
            if (sampleSize == 0)
            {
                throw new ArgumentException("没有足够的测试数据来计算过程能力指数");
            }

            // 计算均值
            double mean = testValues.Average();

            // 计算标准差
            double standardDeviation = CalculateStandardDeviation(testValues, mean);

            // 计算Cp
            double cp = (request.UpperSpecLimit - request.LowerSpecLimit) / (6 * standardDeviation);

            // 计算Cpu和Cpl
            double cpu = (request.UpperSpecLimit - mean) / (3 * standardDeviation);
            double cpl = (mean - request.LowerSpecLimit) / (3 * standardDeviation);

            // 计算Cpk
            double cpk = Math.Min(cpu, cpl);

            // 计算Ca
            double ca = 0;
            if (request.TargetValue.HasValue)
            {
                double target = request.TargetValue.Value;
                double specRange = request.UpperSpecLimit - request.LowerSpecLimit;
                if (specRange > 0)
                {
                    ca = Math.Abs(mean - target) / (specRange / 2);
                }
            }

            // 获取站点信息
            var station = await _stationListRepo.GetQueryable().FirstOrDefaultAsync(s => s.StationId == request.StationId);

            return new ProcessCapabilityDto
            {
                StationId = request.StationId,
                StationCode = station?.StationCode,
                ParametricKey = request.ParametricKey,
                SampleSize = sampleSize,
                Mean = mean,
                StandardDeviation = standardDeviation,
                UpperSpecLimit = request.UpperSpecLimit,
                LowerSpecLimit = request.LowerSpecLimit,
                Cp = cp,
                Cpk = cpk,
                Ca = ca,
                Cpu = cpu,
                Cpl = cpl
            };
        }

        /// <summary>
        /// 获取TOP缺陷分析
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="topN">返回前N个缺陷（默认10）</param>
        /// <param name="productionLineId">生产线ID（可选）</param>
        /// <returns>TOP缺陷分析结果</returns>
        public async Task<List<TopDefectDto>> GetTopDefectsAsync(DateTime startTime, DateTime endTime, int topN = 10, Guid? productionLineId = null)
        {
            // 查询指定时间段内的所有缺陷记录
            var defectQuery = _dbContext.DefectRecords
                .Where(d => d.FoundTime >= startTime && d.FoundTime <= endTime);

            // 注意：DefectRecord实体中没有ProductionLineId字段，因此移除生产线过滤
            // 如需按生产线过滤，需要在DefectRecord实体中添加ProductionLineId字段

            // 按缺陷类型分组统计数量
            var defectStats = await defectQuery
                .GroupBy(d => d.DefectType)
                .Select(g => new
                {
                    DefectType = g.Key,
                    DefectCount = g.Count()
                })
                .OrderByDescending(g => g.DefectCount)
                .Take(topN)
                .ToListAsync();

            // 计算总缺陷数量
            int totalDefectCount = defectStats.Sum(s => s.DefectCount);

            // 构建结果并计算比例
            double cumulativePercentage = 0;
            var result = new List<TopDefectDto>();

            foreach (var stat in defectStats)
            {
                double defectPercentage = totalDefectCount > 0 ? (double)stat.DefectCount / totalDefectCount : 0;
                cumulativePercentage += defectPercentage;

                result.Add(new TopDefectDto
                {
                    DefectType = stat.DefectType,
                    DefectCount = stat.DefectCount,
                    DefectPercentage = defectPercentage,
                    CumulativePercentage = cumulativePercentage
                });
            }

            return result;
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        /// <param name="values">数值列表</param>
        /// <param name="mean">均值</param>
        /// <returns>标准差</returns>
        private double CalculateStandardDeviation(List<double> values, double mean)
        {
            if (values.Count <= 1)
            {
                return 0;
            }

            double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            double variance = sumOfSquares / (values.Count - 1);
            return Math.Sqrt(variance);
        }
    }
}