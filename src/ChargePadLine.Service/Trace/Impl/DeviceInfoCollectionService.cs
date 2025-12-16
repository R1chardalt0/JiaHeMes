using Microsoft.FSharp.Core;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.Service.Trace.Impl
{
    public class DeviceInfoCollectionService : IDeviceInfoCollectionService
    {
        private readonly IRepository<EquipmentTracinfo> _deptRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DeviceInfoCollectionService> _logger;

        public DeviceInfoCollectionService(IRepository<EquipmentTracinfo> deptRepo, AppDbContext dbContext, ILogger<DeviceInfoCollectionService> logger)
        {
            _deptRepo = deptRepo;
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task<FSharpResult<ValueTuple, (int, string)>> DeviceDataCollectionExAsync(string deviceEnCode, DateTimeOffset sendTime, string alarmMessages, List<Iotdata> updateParams)
        {
            try
            {
                // 1. 输入验证
                if (string.IsNullOrWhiteSpace(deviceEnCode))
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "设备编码不能为空"));
                // 2. 校验设备编码是否存在
                var deviceInfo = GetDeviceinfoByDevcideEnCode(deviceEnCode);
                if (deviceInfo == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{deviceEnCode}不存在"));
                }
                // 3. 构造实体并保存
                var traceInfo = new EquipmentTracinfo
                {
                    EquipmentTraceId = Guid.NewGuid(),
                    DeviceEnCode = deviceEnCode,
                    SendTime = sendTime,
                    AlarmMessages = alarmMessages,
                    CreateTime = DateTimeOffset.Now,
                    Parameters = updateParams
                };
                var s = await this._deptRepo.InsertAsyncs(traceInfo);

                if (s > 0)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewOk(new ValueTuple());
                }
                else
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{deviceEnCode},数据上传失败"));
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
