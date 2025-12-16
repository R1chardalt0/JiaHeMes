
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service;

namespace ChargePadLine.Service.Trace
{
    public interface IProductionLineService
    {
        /// <summary>
        /// 分页查询生产线列表
        /// </summary>
        Task<PaginatedList<ProductionLine>> PaginationAsync(int current, int pageSize, string? productionLineName, string? productionLineCode, int? companyId, DateTime? startTime, DateTime? endTime);

        /// <summary>
        /// 获取生产线详情
        /// </summary>
        Task<ProductionLine> GetProductionLineById(Guid productionLineId);

        /// <summary>
        /// 创建生产线
        /// </summary>
        Task<int> CreateProductionLine(ProductionLine productionLine);

        /// <summary>
        /// 更新生产线
        /// </summary>
        Task<int> UpdateProductionLine(ProductionLine productionLine);

        /// <summary>
        /// 批量删除生产线
        /// </summary>
        Task<int> DeleteProductionLineByIds(Guid[] productionLineIds);

        /// <summary>
        /// 获取所有生产线列表
        /// </summary>
        Task<List<ProductionLine>> GetAllProductionLines();
    }
}
