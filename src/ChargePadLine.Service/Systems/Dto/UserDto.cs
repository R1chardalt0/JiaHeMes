using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems.Dto
{
    /// <summary>
    /// 用户查询DTO
    /// </summary>
    public class UserQueryDto
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string? Status { get; set; }
    }

    /// <summary>
    /// 用户新增DTO
    /// </summary>
    public class UserAddDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(50, ErrorMessage = "密码长度不能超过50个字符")]
        public string Password { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [Required(ErrorMessage = "部门ID不能为空")]
        public long DeptId { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
        public string? Email { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [Phone(ErrorMessage = "手机号码格式不正确")]
        [StringLength(20, ErrorMessage = "手机号码长度不能超过20个字符")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 状态 (0正常 1停用)
        /// </summary>
        [Required(ErrorMessage = "状态不能为空")]
        [RegularExpression("^[01]$", ErrorMessage = "状态只能是0或1")]
        public string Status { get; set; }

        /// <summary>
        /// 角色ID数组
        /// </summary>
        public long[]? RoleIds { get; set; }

        /// <summary>
        /// 岗位ID数组
        /// </summary>
        public long[]? PostIds { get; set; }

        /// <summary>
        /// 单个岗位ID（兼容旧版）
        /// </summary>
        public long? PostId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? NickName { get; set; }
    }

    /// <summary>
    /// 用户更新DTO
    /// </summary>
    public class UserUpdateDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required(ErrorMessage = "用户ID不能为空")]
        public long UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string? UserName { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        public long? DeptId { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
        public string? Email { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [Phone(ErrorMessage = "手机号码格式不正确")]
        [StringLength(20, ErrorMessage = "手机号码长度不能超过20个字符")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 状态 (0正常 1停用)
        /// </summary>
        [RegularExpression("^[01]$", ErrorMessage = "状态只能是0或1")]
        public string? Status { get; set; }

        /// <summary>
        /// 角色ID数组
        /// </summary>
        public long[]? RoleIds { get; set; }

        /// <summary>
        /// 岗位ID数组
        /// </summary>
        public long[]? PostIds { get; set; }

        /// <summary>
        /// 单个岗位ID（兼容旧版）
        /// </summary>
        public long? PostId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? NickName { get; set; }
    }

    /// <summary>
    /// 重置密码DTO
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// 新密码
        /// </summary>
        [Required(ErrorMessage = "新密码不能为空")]
        [StringLength(50, ErrorMessage = "密码长度不能超过50个字符")]
        public string NewPassword { get; set; }
    }
}
