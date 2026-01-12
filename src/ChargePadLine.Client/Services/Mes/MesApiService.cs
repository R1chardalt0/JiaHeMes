using ChargePadLine.Client.Services.Mes.Model;
using log4net.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
                var result = await _apiClient.PostAsync<ReqDto, RespDto>("/api/CommonInterfase/UploadCheck", req);
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
                var result = await _apiClient.PostAsync<ReqDto, RespDto>("/api/CommonInterfase/UploadData", req);
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
