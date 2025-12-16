using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Service.Trace.Dto;

namespace ChargePadLine.Service.Trace.Impl
{
    public class ProductTraceInfoCollectionService : IProductTraceInfoCollectionService
    {
        private readonly IRepository<ProductTraceInfo> _ptinfoRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ProductTraceInfoCollectionService> _logger;

        public ProductTraceInfoCollectionService(IRepository<ProductTraceInfo> ptinfoRepo, AppDbContext dbContext, ILogger<ProductTraceInfoCollectionService> logger)
        {
            _ptinfoRepo = ptinfoRepo;
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task<FSharpResult<ValueTuple, (int, string)>> DataCollectForSfcExAsync(RequestParametricData request)
        {
            try
            {
                // 1. 输入验证
                if (string.IsNullOrWhiteSpace(request.Resource))
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "设备编码不能为空"));
                // 2. 校验设备编码是否存在
                var deviceInfo = GetDeviceinfoByDevcideEnCode(request.Resource);
                if (deviceInfo == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource}不存在"));
                }
                // 3. 构造实体并保存
                var traceInfo = new ProductTraceInfo
                {
                    ProductTraceId = Guid.NewGuid(),                   
                    Site​​ = request.Site,
                    ActivityId = request.ActivityId,
                    Resource = request.Resource,
                    DcGroupRevision = request.DcGroupRevision,
                    IsOK = request.IsOK,
                    Sfc = request.Sfc,
                    SendTime = request.SendTime,
                    CreateTime = DateTimeOffset.Now,
                    parametricDataArray = request.parametricDataArray
                };
                var s = await this._ptinfoRepo.InsertAsyncs(traceInfo);

                if (s > 0)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewOk(new ValueTuple());
                }
                else
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},数据上传失败"));
                }
            }
            catch (Exception e)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"DataCollectForSfcExAsync接口出错{e.Message}"));
            }
        }

        public DeviceInfo GetDeviceinfoByDevcideEnCode(string deviceEnCode)
        {
            if (string.IsNullOrEmpty(deviceEnCode))
            {
                return null;
            }
            return _dbContext.DeviceInfos.FirstOrDefault(d => d.DeviceEnCode == deviceEnCode);
        }


    }
}
