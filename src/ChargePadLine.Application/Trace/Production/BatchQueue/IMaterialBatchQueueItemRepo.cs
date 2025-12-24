using ChargePadLine.Entitys.Trace.Production.BatchQueue;
using ChargePadLine.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Application.Trace.Production.BatchQueue
{
    public record Qry_物料批_队列(
      int PageIndex,
      int PageSize,

      int Id,
      string? BomItemCode
  );

    public interface IMaterialBatchQueueItemRepo 
    {
        /// <summary>
        /// 检查指定批次是否已经存在(不管是否已经删除). 如果存在,返回第一个匹配的队列项; 否则返回null
        /// </summary>
        /// <param name="bomItemCode"></param>
        /// <param name="batchNum"></param>
        /// <returns></returns>
        Task<BatchMaterialQueueItem?> CheckBatchExistsAsync(string bomItemCode, string batchNum);

        /// <summary>
        /// 找到最多前N个用于扣料的候选项
        /// </summary>
        /// <param name="bomItemCode"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IList<BatchMaterialQueueItem>> FindTopCandicatesAsync(string bomItemCode, int limit);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        Task<PaginatedList<BatchMaterialQueueItem>> PaginateAsync(Qry_物料批_队列 qry);
    }
}
