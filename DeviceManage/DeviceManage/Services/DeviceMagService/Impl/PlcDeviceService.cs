using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
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
        private ILogger<PlcDeviceService> _logger;


        public PlcDeviceService(IRepository<PlcDevice> plcDeviceRepo, AppDbContext dbContext, ILogger<PlcDeviceService> logge)
        {
            _plcDeviceRepo = plcDeviceRepo;
            _dbContext = dbContext;
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
            return plcDevice;
        }

        public async Task<PlcDevice> UpdatePlcDeviceAsync(PlcDevice plcDevice)
        {
            _plcDeviceRepo.Update(plcDevice);
            await _dbContext.SaveChangesAsync();
            return plcDevice;
        }

        public async Task DeletePlcDeviceAsync(int id)
        {
            var plcDevice = await _plcDeviceRepo.GetAsync(r => r.Id == id);
            if (plcDevice != null)
            {
                _plcDeviceRepo.Delete(plcDevice);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning($"尝试删除不存在的PLC设备，ID: {id}");
            }
        }
    }
}
