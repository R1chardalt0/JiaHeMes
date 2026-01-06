using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    public interface IStationListService
    {
        /// <summary>
        /// 获取站点分页数据
        /// </summary>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="stationName"></param>
        /// <param name="stationCode"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<PaginatedList<StationListDto>> PaginationAsync(StationListQueryDto queryDto);
        /// <summary>
        /// 根据Id获取站点信息
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        Task<StationListDto> GetStationInfoById(Guid stationId);
        /// <summary>
        /// 根据站点编码获取站点信息
        /// </summary>
        /// <param name="stationCode"></param>
        /// <returns></returns>
        Task<StationListDto> GetStationInfoByCode(string stationCode);
        /// <summary>
        /// 获取站点列表
        /// </summary>
        /// <returns></returns>


        Task<StationListDto> CreateStationInfo(StationListDto dto);
        /// <summary>
        /// 修改站点信息
        /// </summary>
        /// <param name="stationInfo"></param>
        /// <returns></returns>

        Task<StationListDto> UpdateStationInfo(StationListDto stationInfo);

        /// <summary>
        /// 删除站点信息
        /// </summary>
        /// <param name="stationIds"></param>
        /// <returns></returns>
        Task<bool> DeleteStationInfoById(Guid id);
        /// <summary>
        /// 获取所有站点信息
        /// </summary>
        /// <returns></returns>

        Task<List<StationListDto>> GetAllStationInfos();
 
    }
}
