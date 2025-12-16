using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Systems
{
    /// <summary>
    /// 系统操作日志实体类
    /// 用于记录用户对业务数据的增删改操作
    /// </summary>
    public class SysOperationLog
    {
        /// <summary>
        /// 日志ID（主键，自增）
        /// </summary>
        [Key]
        [Column("log_id")]
        [Description("日志ID（主键，自增）")]
        public long LogId { get; set; }

        /// <summary>
        /// 操作用户工号（非空，关联用户表工号字段）
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("user_code")]
        [Description("操作用户工号（非空，关联用户表工号字段）")]
        public string UserCode { get; set; } = string.Empty;

        /// <summary>
        /// 操作用户姓名（非空，冗余存储，避免用户表数据变更后日志失效）
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("user_name")]
        [Description("操作用户姓名（非空，冗余存储，避免用户表数据变更后日志失效）")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型（非空，枚举值：INSERT-新增、UPDATE-修改、DELETE-删除）
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("operation_type")]
        [Description("操作类型（非空，枚举值：INSERT-新增、UPDATE-修改、DELETE-删除）")]
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// 操作模块（非空，如：用户管理、订单管理、产品配置等，说明操作归属的业务模块）
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("operation_module")]
        [Description("操作模块（非空，如：用户管理、订单管理、产品配置等，说明操作归属的业务模块）")]
        public string OperationModule { get; set; } = string.Empty;

        /// <summary>
        /// 操作对象ID（非空，被操作数据的主键ID，如：修改的用户ID、删除的订单ID）
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("target_id")]
        [Description("操作对象ID（非空，被操作数据的主键ID，如：修改的用户ID、删除的订单ID）")]
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 操作前数据（可选，JSON格式存储，记录修改/删除前的原始数据，便于回溯）
        /// </summary>
        [Column("before_data")]
        [Description("操作前数据（可选，JSON格式存储，记录修改/删除前的原始数据，便于回溯）")]
        public string? BeforeData { get; set; }

        /// <summary>
        /// 操作后数据（可选，JSON格式存储，记录新增/修改后的目标数据）
        /// </summary>
        [Column("after_data")]
        [Description("操作后数据（可选，JSON格式存储，记录新增/修改后的目标数据）")]
        public string? AfterData { get; set; }

        /// <summary>
        /// 操作时间（非空，默认当前时间戳，精确到秒）
        /// </summary>
        [Required]
        [Column("operation_time")]
        [Description("操作时间（非空，默认当前时间戳，精确到秒）")]
        public DateTimeOffset OperationTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 操作IP地址（非空，记录用户操作时的客户端IP）
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("operation_ip")]
        [Description("操作IP地址（非空，记录用户操作时的客户端IP）")]
        public string OperationIp { get; set; } = string.Empty;

        /// <summary>
        /// 操作备注（可选，用户手动输入的操作说明，或系统自动生成的备注，如："批量删除过期订单"）
        /// </summary>
        [Column("operation_remark")]
        [Description("操作备注（可选，用户手动输入的操作说明，或系统自动生成的备注，如：\"批量删除过期订单\"）")]
        public string? OperationRemark { get; set; }

        /// <summary>
        /// 操作状态（非空，枚举值：SUCCESS-成功、FAIL-失败，记录操作是否执行成功）
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("operation_status")]
        [Description("操作状态（非空，枚举值：SUCCESS-成功、FAIL-失败，记录操作是否执行成功）")]
        public string OperationStatus { get; set; } = "SUCCESS";
    }

    /// <summary>
    /// 操作类型枚举
    /// </summary>
    public static class OperationType
    {
        /// <summary>
        /// 新增操作
        /// </summary>
        public const string INSERT = "INSERT";

        /// <summary>
        /// 修改操作
        /// </summary>
        public const string UPDATE = "UPDATE";

        /// <summary>
        /// 删除操作
        /// </summary>
        public const string DELETE = "DELETE";

        /// <summary>
        /// 登录操作
        /// </summary>
        public const string LOGIN = "LOGIN";
    }

    /// <summary>
    /// 操作状态枚举
    /// </summary>
    public static class OperationStatus
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        public const string SUCCESS = "SUCCESS";

        /// <summary>
        /// 操作失败
        /// </summary>
        public const string FAIL = "FAIL";
    }
}

