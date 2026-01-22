using ChargePadLine.Client.Services.Mes.Dto;
using log4net.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.Mes
{
    public class MesApiService : IMesApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<MesApiService> _logger;
        public MesApiService(ApiClient apiClient, ILogger<MesApiService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<RespDto> UploadCheck(ReqDto req)
        {
            try
            {
                var request = new UploadCheckRequestDto
                {
                    SN = req.sn,
                    Resource = req.resource,
                    StationCode = req.stationCode,
                    WorkOrderCode = req.workOrderCode ?? string.Empty,
                    TestResult = req.testResult ?? string.Empty,
                    TestData = JsonSerializer.Serialize(req.testData ?? new List<TestDataItem>())
                };

                var result = await _apiClient.PostAsync<UploadCheckRequestDto, RespDto>("/api/CommonInterfase/UploadCheck", request);
                return result ?? new RespDto { code = -1, message = "No response received" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading check data");
                return new RespDto { code = -1, message = ex.Message };
            }
        }


        public async Task<RespDto> UploadData(ReqDto req)
        {
            try
            {
                var request = new UploadCheckRequestDto
                {
                    SN = req.sn,
                    Resource = req.resource,
                    StationCode = req.stationCode,
                    WorkOrderCode = req.workOrderCode ?? string.Empty,
                    TestResult = req.testResult ?? string.Empty,
                    TestData = JsonSerializer.Serialize(req.testData ?? new List<TestDataItem>())
                };
                var result = await _apiClient.PostAsync<UploadCheckRequestDto, RespDto>("api/CommonInterfase/UploadData", request);
                return result ?? new RespDto { code = -1, message = "No response received" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading data");
                return new RespDto { code = -1, message = ex.Message };
            }
        }

        public async Task<RespDto> UploadMaster(ReqDto req)
        {
            try
            {
                var request = new UploadCheckRequestDto
                {
                    SN = req.sn,
                    Resource = req.resource,
                    StationCode = req.stationCode,
                    WorkOrderCode = req.workOrderCode ?? string.Empty,
                    TestResult = req.testResult ?? string.Empty,
                    TestData = JsonSerializer.Serialize(req.testData ?? new List<TestDataItem>())
                };
                var result = await _apiClient.PostAsync<UploadCheckRequestDto, RespDto>("api/CommonInterfase/UploadMaster", request);
                return result ?? new RespDto { code = -1, message = "No response received" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading data");
                return new RespDto { code = -1, message = ex.Message };
            }
        }
    }
}
