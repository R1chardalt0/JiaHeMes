using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceManage.Models
{
    /// <summary>
    /// 操作日志表
    /// </summary>
    [Table("dm_operation_log")]
    public class OperationLog : BaseModel
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID（关联用户表）
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 用户名（冗余字段，方便查询）
        /// </summary>
        [MaxLength(50)]
        public string? Username { get; set; }

        /// <summary>
        /// 操作类型（枚举类型，不映射到数据库）
        /// </summary>
        [NotMapped]
        public OperationType OperationType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OperationTypeString))
                    return OperationType.Other;

                if (Enum.TryParse<OperationType>(OperationTypeString, true, out var parsedType))
                    return parsedType;

                return OperationType.Other;
            }
            set
            {
                OperationTypeString = value.ToString();
            }
        }

        /// <summary>
        /// 操作类型（数据库存储字符串）
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("OperationType")]
        public string OperationTypeString { get; set; } = string.Empty;

        /// <summary>
        /// 模块名称（如：用户管理、设备管理等）
        /// </summary>
        [MaxLength(100)]
        public string? Module { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 操作结果（成功/失败）
        /// </summary>
        [MaxLength(20)]
        public string? Result { get; set; }

        /// <summary>
        /// 请求参数（JSON格式，存储操作相关的参数）
        /// </summary>
        [Column(TypeName = "text")]
        public string? RequestParams { get; set; }

        /// <summary>
        /// 错误信息（如果操作失败，记录错误信息）
        /// </summary>
        [Column(TypeName = "text")]
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 操作类型枚举
    /// </summary>
    public enum OperationType
    {
        [Description("登录")]
        Login = 1,

        [Description("登出")]
        Logout = 2,

        [Description("创建")]
        Create = 3,

        [Description("更新")]
        Update = 4,

        [Description("删除")]
        Delete = 5,

        [Description("查询")]
        Query = 6,

        [Description("导出")]
        Export = 7,

        [Description("导入")]
        Import = 8,

        [Description("其他")]
        Other = 99
    }
}

