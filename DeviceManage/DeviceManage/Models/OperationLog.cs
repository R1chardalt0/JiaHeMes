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
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [MaxLength(50)]
        public string? Username { get; set; }

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

        [Required]
        [MaxLength(50)]
        [Column("OperationType")]
        public string OperationTypeString { get; set; } = string.Empty;

        [NotMapped]
        public string OperationTypeDisplay => EnumDescriptionHelper.GetDescription(OperationType);

        [MaxLength(100)]
        public string? Module { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(20)]
        public string? Result { get; set; }

        [Column(TypeName = "text")]
        public string? RequestParams { get; set; }

        [Column(TypeName = "text")]
        public string? ErrorMessage { get; set; }
    }

    public static class EnumDescriptionHelper
    {
        public static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }
    }

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
