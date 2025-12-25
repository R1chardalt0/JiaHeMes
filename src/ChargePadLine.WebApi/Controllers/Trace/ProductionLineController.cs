using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    public class ProductionLineController : BaseController
    {
        private readonly IProductionLineService _productionLineService;

        public ProductionLineController(IProductionLineService productionLineService)
        {
            _productionLineService = productionLineService;
        }

        /// <summary>
        /// 分页查询生产线列表
        /// </summary>
        [HttpGet]
        public async Task<PagedResp<ProductionLine>> GetProductionLineList(int current, int pageSize, string? productionLineName, string? productionLineCode, DateTime? startTime, DateTime? endTime)
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
                var list = await _productionLineService.PaginationAsync(current, pageSize, productionLineName, productionLineCode, startTime, endTime);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakePagedEmpty<ProductionLine>();
            }
        }

        /// <summary>
        /// 根据生产线ID获取生产线详情
        /// </summary>
        [HttpGet("{productionLineId}")]
        public async Task<Resp<ProductionLine>> GetProductionLineById(Guid productionLineId)
        {
            try
            {
                var productionLine = await _productionLineService.GetProductionLineById(productionLineId);
                if (productionLine == null)
                {
                    return RespExtensions.MakeFail<ProductionLine>("404", "生产线不存在");
                }
                return RespExtensions.MakeSuccess(productionLine);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<ProductionLine>("500", ex.Message);
            }
        }

        /// <summary>
        /// 创建生产线
        /// </summary>
        [HttpPost]
        public async Task<Resp<bool>> CreateProductionLine([FromBody] ProductionLine productionLine)
        {
            try
            {
                // 验证参数
                if (productionLine == null || string.IsNullOrEmpty(productionLine.ProductionLineName))
                {
                    return RespExtensions.MakeFail<bool>("400", "生产线名称不能为空");
                }

                // 生成新的生产线ID
                productionLine.ProductionLineId = Guid.NewGuid();

                var result = await _productionLineService.CreateProductionLine(productionLine);
                if (result == -1)
                {
                    return RespExtensions.MakeFail<bool>("400", "生产线名称已存在");
                }
                return RespExtensions.MakeSuccess(result > 0);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<bool>("500", ex.Message);
            }
        }

        /// <summary>
        /// 更新生产线
        /// </summary>
        [HttpPost]
        public async Task<Resp<bool>> UpdateProductionLine([FromBody] ProductionLine productionLine)
        {
            try
            {
                // 验证参数
                if (productionLine == null || productionLine.ProductionLineId == Guid.Empty || string.IsNullOrEmpty(productionLine.ProductionLineName))
                {
                    return RespExtensions.MakeFail<bool>("400", "生产线ID和名称不能为空");
                }

                var result = await _productionLineService.UpdateProductionLine(productionLine);
                if (result == -1)
                {
                    return RespExtensions.MakeFail<bool>("400", "生产线名称已存在");
                }
                return RespExtensions.MakeSuccess(result > 0);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<bool>("500", ex.Message);
            }
        }

        /// <summary>
        /// 批量删除生产线
        /// </summary>
        [HttpPost]
        public async Task<Resp<bool>> DeleteProductionLineByIds([FromBody] List<Guid> productionLineIds)
        {
            try
            {
                if (productionLineIds == null || productionLineIds.Count == 0)
                {
                    return RespExtensions.MakeFail<bool>("400", "请选择要删除的生产线");
                }

                var result = await _productionLineService.DeleteProductionLineByIds(productionLineIds.ToArray());
                return RespExtensions.MakeSuccess(result > 0);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<bool>("500", ex.Message);
            }
        }

        /// <summary>
        /// 获取所有生产线列表
        /// </summary>
        [HttpGet("All")]
        public async Task<Resp<List<ProductionLine>>> GetAllProductionLines()
        {
            try
            {
                var list = await _productionLineService.GetAllProductionLines();
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<ProductionLine>>("500", ex.Message);
            }
        }
    }
}


