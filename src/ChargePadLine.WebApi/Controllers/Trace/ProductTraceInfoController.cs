using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;
using ChargePadLine.WebApi.util;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.WebApi.Controllers.Systems;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    public class ProductTraceInfoController : BaseController
    {
        private readonly IProductTraceInfoService _productTraceInfoService;
        public ProductTraceInfoController(IProductTraceInfoService productTraceInfoService)
        {
            _productTraceInfoService = productTraceInfoService;
        }

        /// <summary>
        /// 分页查询产品追溯信息
        /// </summary>
        [HttpGet]
        public async Task<PagedResp<ProductTraceInfoDto>> GetProductTraceInfoList(int current, int pageSize, string? sfc, string? productionLine, string? deviceName, string? resource, DateTime? startTime, DateTime? endTime)
        {
            try
            {
                if (current < 1)
                {
                    current = 1;
                }
                if (pageSize < 1)
                {
                    pageSize = 50;
                }
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                var list = await _productTraceInfoService.PaginationAsync(current, pageSize, sfc, productionLine, deviceName, resource, startTime, endTime);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<ProductTraceInfoDto>();
            }
        }

        /// <summary>
        /// 根据设备编码获取产品追溯信息列表
        /// </summary>
        [HttpGet]
        public async Task<Resp<List<ProductTraceInfo>>> GetProductTraceInfoListByResource(string resource, int size)
        {
            try
            {
                var list = await _productTraceInfoService.GetProductTraceInfoListByResource(resource, size);
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<ProductTraceInfo>>("500", ex.Message);
            }
        }

        /// <summary>
        /// 根据产品编码获取最新的产品追溯信息
        /// </summary>
        [HttpGet]
        public async Task<Resp<ProductTraceInfo>> GetProductTraceInfoBySfc(string sfc,string resourse)
        {
            try
            {
                var productTraceInfo = await _productTraceInfoService.GetProductTraceInfoBySfc(sfc, resourse);
                return RespExtensions.MakeSuccess(productTraceInfo);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<ProductTraceInfo>("500", ex.Message);
            }
        }

        /// <summary>
        /// 获取指定产品在各工站的最新追溯数据
        /// </summary>
        /// <param name="sfc">产品编码</param>
        /// <returns>按资源分组的最新追溯数据</returns>
        [HttpGet]
        public async Task<Resp<List<ProductTraceInfo>>> GetLatestProductTraceInfoBySfcGroupedByResource(string sfc)
        {
            try
            {
                var list = await _productTraceInfoService.GetLatestProductTraceInfoBySfcGroupedByResource(sfc);
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<ProductTraceInfo>>("500", ex.Message);
            }
        }

        /// <summary>
        /// 查询生产线产量记录
        /// </summary>
        /// <param name="productionLineName">生产线名称</param>
        /// <param name="deviceName">设备名称</param>
        /// <param name="resource">资源编码</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>产量记录列表</returns>
        [HttpGet]
        public async Task<Resp<List<ProductionRecordsDto>>> GetProductionRecordsAsync(string? productionLineName, string? deviceName, string? resource, DateTime startTime, DateTime endTime)
        {
            try
            {
                var list = await _productTraceInfoService.GetProductionRecordsAsync(productionLineName, deviceName, startTime,endTime,resource);
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<ProductionRecordsDto>>("500", ex.Message);
            }
        }
    }
}
