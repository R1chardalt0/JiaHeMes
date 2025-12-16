using System;
using System.ComponentModel;

namespace ChargePadLine.Service.OperationLog.Dto
{
    /// <summary>
    /// 操作日志查询DTO
    /// </summary>
    public class OperationLogQueryDto
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        [Description("当前页码")]
        public int Current { get; set; } = 1;

        /// <summary>
        /// 每页条数
        /// </summary>
        [Description("每页条数")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 关键字搜索（工号、姓名、操作模块、操作对象ID）
        /// </summary>
        [Description("关键字搜索（工号、姓名、操作模块、操作对象ID）")]
        public string? Keyword { get; set; }

        /// <summary>
        /// 操作用户工号
        /// </summary>
        [Description("操作用户工号")]
        public string? UserCode { get; set; }

        /// <summary>
        /// 操作用户姓名
        /// </summary>
        [Description("操作用户姓名")]
        public string? UserName { get; set; }

        /// <summary>
        /// 操作类型（INSERT、UPDATE、DELETE）
        /// </summary>
        [Description("操作类型（INSERT、UPDATE、DELETE）")]
        public string? OperationType { get; set; }

        /// <summary>
        /// 操作模块
        /// </summary>
        [Description("操作模块")]
        public string? OperationModule { get; set; }

        /// <summary>
        /// 操作对象ID
        /// </summary>
        [Description("操作对象ID")]
        public string? TargetId { get; set; }

        /// <summary>
        /// 操作状态（SUCCESS、FAIL）
        /// </summary>
        [Description("操作状态（SUCCESS、FAIL）")]
        public string? OperationStatus { get; set; }

        /// <summary>
        /// 操作时间开始（可选）
        /// </summary>
        [Description("操作时间开始")]
        public DateTimeOffset? OperationTimeStart { get; set; }

        /// <summary>
        /// 操作时间结束（可选）
        /// </summary>
        [Description("操作时间结束")]
        public DateTimeOffset? OperationTimeEnd { get; set; }
    }

    /// <summary>
    /// 操作日志新增DTO
    /// </summary>
    public class OperationLogAddDto
    {
        /// <summary>
        /// 操作用户工号（非空，关联用户表工号字段）
        /// </summary>
        [Description("操作用户工号（非空，关联用户表工号字段）")]
        public string UserCode { get; set; } = string.Empty;

        /// <summary>
        /// 操作用户姓名（非空，冗余存储，避免用户表数据变更后日志失效）
        /// </summary>
        [Description("操作用户姓名（非空，冗余存储，避免用户表数据变更后日志失效）")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型（非空，枚举值：INSERT-新增、UPDATE-修改、DELETE-删除）
        /// </summary>
        [Description("操作类型（非空，枚举值：INSERT-新增、UPDATE-修改、DELETE-删除）")]
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// 操作模块（非空，如：用户管理、订单管理、产品配置等，说明操作归属的业务模块）
        /// </summary>
        [Description("操作模块（非空，如：用户管理、订单管理、产品配置等，说明操作归属的业务模块）")]
        public string OperationModule { get; set; } = string.Empty;

        /// <summary>
        /// 操作对象ID（非空，被操作数据的主键ID，如：修改的用户ID、删除的订单ID）
        /// </summary>
        [Description("操作对象ID（非空，被操作数据的主键ID，如：修改的用户ID、删除的订单ID）")]
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 操作前数据（可选，JSON格式存储，记录修改/删除前的原始数据，便于回溯）
        /// </summary>
        [Description("操作前数据（可选，JSON格式存储，记录修改/删除前的原始数据，便于回溯）")]
        public string? BeforeData { get; set; }

        /// <summary>
        /// 操作后数据（可选，JSON格式存储，记录新增/修改后的目标数据）
        /// </summary>
        [Description("操作后数据（可选，JSON格式存储，记录新增/修改后的目标数据）")]
        public string? AfterData { get; set; }

        /// <summary>
        /// 操作IP地址（非空，记录用户操作时的客户端IP）
        /// </summary>
        [Description("操作IP地址（非空，记录用户操作时的客户端IP）")]
        public string OperationIp { get; set; } = string.Empty;

        /// <summary>
        /// 操作备注（可选，用户手动输入的操作说明，或系统自动生成的备注，如："批量删除过期订单"）
        /// </summary>
        [Description("操作备注（可选，用户手动输入的操作说明，或系统自动生成的备注，如：\"批量删除过期订单\"）")]
        public string? OperationRemark { get; set; }

        /// <summary>
        /// 操作状态（非空，枚举值：SUCCESS-成功、FAIL-失败，记录操作是否执行成功）
        /// </summary>
        [Description("操作状态（非空，枚举值：SUCCESS-成功、FAIL-失败，记录操作是否执行成功）")]
        public string OperationStatus { get; set; } = "SUCCESS";
    }
}

