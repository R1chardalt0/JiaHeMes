using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Helpers;
using DeviceManage.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService.Impl
{
    public class PlcDeviceService : IPlcDeviceService
    {
        private readonly IRepository<PlcDevice> _plcDeviceRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogService _logService;
        private ILogger<PlcDeviceService> _logger;


        public PlcDeviceService(IRepository<PlcDevice> plcDeviceRepo, AppDbContext dbContext, ILogService logService, ILogger<PlcDeviceService> logge)
        {
            _plcDeviceRepo = plcDeviceRepo;
            _dbContext = dbContext;
            _logService = logService;
            _logger = logge;
        }

        public async Task<List<PlcDevice>> GetAllPlcDevicesAsync()
        {
            return await _plcDeviceRepo.GetListAsync();
        }

        public async Task<PlcDevice?> GetPlcDeviceByIdAsync(int id)
        {
            return await _plcDeviceRepo.GetAsync(r => r.Id == id);
        }

        public async Task<PlcDevice> AddPlcDeviceAsync(PlcDevice plcDevice)
        {
            await _plcDeviceRepo.InsertAsync(plcDevice);
            await _dbContext.SaveChangesAsync();

            await _logService.LogAsync(
                CurrentUserContext.UserId,
                CurrentUserContext.Username,
                OperationType.Create,
                "PLC设备管理",
                $"新增PLC设备：{plcDevice.PLCName} (ID:{plcDevice.Id})");

            return plcDevice;
        }

        public async Task<PlcDevice> UpdatePlcDeviceAsync(PlcDevice plcDevice)
        {
            var existingDevice = await _plcDeviceRepo.GetAsync(r => r.Id == plcDevice.Id);
            if (existingDevice == null)
            {
                _logger.LogWarning($"尝试更新不存在的PLC设备，ID: {plcDevice.Id}");
                throw new InvalidOperationException($"PLC设备不存在，ID: {plcDevice.Id}");
            }
            existingDevice.PLCName = plcDevice.PLCName;
            existingDevice.IPAddress = plcDevice.IPAddress;
            existingDevice.Port = plcDevice.Port;
            existingDevice.Protocolc = plcDevice.Protocolc;
            existingDevice.Model = plcDevice.Model;
            existingDevice.Remarks = plcDevice.Remarks;

            _plcDeviceRepo.Update(existingDevice);
            await _dbContext.SaveChangesAsync();

            await _logService.LogAsync(
                CurrentUserContext.UserId,
                CurrentUserContext.Username,
                OperationType.Update,
                "PLC设备管理",
                $"修改PLC设备：{existingDevice.PLCName} (ID:{existingDevice.Id})");

            return existingDevice;
        }

        public async Task DeletePlcDeviceAsync(int id)
        {
            var plcDevice = await _plcDeviceRepo.GetAsync(r => r.Id == id);
            if (plcDevice != null)
            {
                var plcName = plcDevice.PLCName;
                var plcId = plcDevice.Id;

                _plcDeviceRepo.Delete(plcDevice);
                await _dbContext.SaveChangesAsync();

                await _logService.LogAsync(
                    CurrentUserContext.UserId,
                    CurrentUserContext.Username,
                    OperationType.Delete,
                    "PLC设备管理",
                    $"删除PLC设备：{plcName} (ID:{plcId})");
            }
            else
            {
                _logger.LogWarning($"尝试删除不存在的PLC设备，ID: {id}");
            }
        }
    }
}
