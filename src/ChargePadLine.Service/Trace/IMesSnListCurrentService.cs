using ChargePadLine.Service;
using ChargePadLine.Service.Trace.Dto.MesSnList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// SN实时状态服务接口
  /// </summary>
  public interface IMesSnListCurrentService
  {
    /// <summary>
    /// 分页查询SN实时状态列表
    /// </summary>
    /// <param name="queryDto">查询条件DTO</param>
    /// <returns>分页查询结果</returns>
    Task<PaginatedList<MesSnListCurrentDto>> GetMesSnListCurrentsAsync(MesSnListCurrentQueryDto queryDto);

    /// <summary>
    /// 根据SNListCurrentId获取SN实时状态详情
    /// </summary>
    /// <param name="sNListCurrentId">SN实时状态ID</param>
    /// <returns>SN实时状态详情</returns>
    Task<MesSnListCurrentDto?> GetMesSnListCurrentByIdAsync(Guid sNListCurrentId);

    /// <summary>
    /// 根据SnNumber获取SN实时状态详情
    /// </summary>
    /// <param name="snNumber">序列号/产品唯一码</param>
    /// <returns>SN实时状态详情</returns>
    Task<MesSnListCurrentDto?> GetMesSnListCurrentBySnNumberAsync(string snNumber);
  }
}