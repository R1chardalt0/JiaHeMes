using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service;

namespace ChargePadLine.Application.Trace.Production.TraceInformation
{
    public interface ITraceInfoRepository
    {
         
        /// <summary>
        /// 根据VSN检索
        /// </summary>
        /// <param name="vsn"></param>
        /// <returns></returns>
        Task<TraceInfo?> FindWithVsnAsync(uint vsn);

        /// <summary>
        /// 根据产品识别码检索
        /// </summary>
        /// <param name="PIN"></param>
        /// <returns></returns>
        Task<TraceInfo?> FindWithPINAsync(SKU PIN);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        Task<PaginatedList<TraceInfo>> PaginateAsync(QryTraceInfoList qry);



        /// <summary>
        /// 检索最大N条过期的记录
        /// </summary>
        /// <param name="expiry">过期时间，以min计算</param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IList<Guid>> SearchExpiredRecordsAsync(int expiry, int limit);

        /// <summary>
        /// 添加TraceInfo记录
        /// </summary>
        /// <param name="traceInfo">TraceInfo对象</param>
        /// <param name="saveChanges">是否保存更改</param>
        /// <returns></returns>
        Task<TraceInfo> AddAsync(TraceInfo traceInfo, bool saveChanges);

        /// <summary>
        /// 根据ID查找TraceInfo
        /// </summary>
        /// <param name="id">TraceInfo ID</param>
        /// <returns></returns>
        Task<TraceInfo?> FindAsync(Guid id);

        /// <summary>
        /// 保存更改
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync();
    }
}

