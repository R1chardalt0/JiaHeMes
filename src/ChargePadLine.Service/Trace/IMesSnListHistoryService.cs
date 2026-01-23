using ChargePadLine.Service.Trace.Dto.MesSnList;
using System;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// SN历史记录服务接口
  /// </summary>
  public interface IMesSnListHistoryService
  {
    /// <summary>
    /// 分页查询SN历史记录列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedList<MesSnListHistoryDto>> GetMesSnListHistoriesAsync(MesSnListHistoryQueryDto queryDto);

    /// <summary>
    /// 根据SNListHistoryId获取SN历史记录详情
    /// </summary>
    /// <param name="sNListHistoryId">状态ID</param>
    /// <returns>SN历史记录详情</returns>
    Task<MesSnListHistoryDto?> GetMesSnListHistoryByIdAsync(Guid sNListHistoryId);

    /// <summary>
    /// 根据SnNumber获取SN历史记录详情
    /// </summary>
    /// <param name="snNumber">序列号</param>
    /// <returns>SN历史记录详情</returns>
    Task<MesSnListHistoryDto?> GetMesSnListHistoryBySnNumberAsync(string snNumber);
  }
}
