using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace.Dto.Station;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// WorkOrder扩展方法
  /// </summary>
  public static class StationListExtensions
  {
    /// <summary>
    /// 转换为Dto
    /// </summary>
    /// <param name="stationList"></param>
    /// <returns></returns>
    public static StationListDto ToDto(this StationList stationList) => new()
    {
      StationId = stationList.StationId,
      StationCode = stationList.StationCode,
      StationName = stationList.StationName
    };
  }
}