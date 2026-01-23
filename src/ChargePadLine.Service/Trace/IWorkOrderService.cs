// 已废弃
// using ChargePadLine.Entitys.Trace.WorkOrders;
// using ChargePadLine.Service.Trace.Dto;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

// namespace ChargePadLine.Service.Trace
// {
//   /// <summary>
//   /// 工单服务接口
//   /// </summary>
//   public interface IWorkOrderService
//   {
//     /// <summary>
//     /// 分页查询工单列表
//     /// </summary>
//     Task<PaginatedList<WorkOrderDto>> GetWorkOrdersAsync(WorkOrderQueryDto queryDto);



//     /// <summary>
//     /// 根据ID获取工单详情
//     /// </summary>
//     /// <param name="id">工单ID</param>
//     /// <returns>工单详情</returns>
//     Task<WorkOrderDto?> GetWorkOrderByIdAsync(int id);

//     /// <summary>
//     /// 根据工单编码获取工单详情
//     /// </summary>
//     /// <param name="code">工单编码</param>
//     /// <returns>工单详情</returns>
//     Task<WorkOrderDto?> GetWorkOrderByCodeAsync(string code);

//     /// <summary>
//     /// 创建工单
//     /// </summary>
//     /// <param name="dto">创建工单DTO</param>
//     /// <returns>创建成功的工单信息</returns>
//     Task<WorkOrderDto> CreateWorkOrderAsync(CreateWorkOrderDto dto);

//     /// <summary>
//     /// 更新工单
//     /// </summary>
//     /// <param name="dto">更新工单DTO</param>
//     /// <returns>更新后的工单信息</returns>
//     Task<WorkOrderDto> UpdateWorkOrderAsync(UpdateWorkOrderDto dto);

//     /// <summary>
//     /// 删除工单
//     /// </summary>
//     /// <param name="id">工单ID</param>
//     /// <returns>删除结果，成功返回true，工单不存在返回false</returns>
//     Task<bool> DeleteWorkOrderAsync(int id);

//     /// <summary>
//     /// 批量删除工单
//     /// </summary>
//     /// <param name="ids">工单ID数组</param>
//     /// <returns>实际删除的工单数量</returns>
//     Task<int> DeleteWorkOrdersAsync(int[] ids);
//   }
// }