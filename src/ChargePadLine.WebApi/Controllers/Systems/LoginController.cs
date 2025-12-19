using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Common.TokenModule.Models;
using ChargePadLine.Common.TokenModule;
using ChargePadLine.Service.Systems;
using ChargePadLine.Service.Systems.Dto;
using ChargePadLine.Service.Systems.Impl;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;
using ChargePadLine.Common.Md5Module;
using System.Collections.Generic;
using ChargePadLine.Service.OperationLog;
using ChargePadLine.Service.OperationLog.Dto;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.WebApi.Controllers.util;

namespace ChargePadLine.WebApi.Controllers.Systems
{
    public class LoginController : BaseController
    {
        public IUserService _userService { get; set; }
        public IConfiguration _configuration { get; }
        private readonly IServiceProvider _serviceProvider;

        public LoginController(IUserService userService, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this._configuration = configuration;
            this._userService = userService;
            this._serviceProvider = serviceProvider;
        }


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        //[HttpPost]
        //public async Task<string> CheckLogin(UserLoginDto loginDto)
        //{
        //    if (string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.PassWord))
        //    {
        //        HttpContext.Response.StatusCode = 400;
        //        return "NonUserInfo";
        //    }
        //    var isSuccess = await _userService.CheckPassword(loginDto);
        //    if (isSuccess)
        //    {
        //        //TODO 获取真实的用户数据  
        //        var user = await _userService.GetCustomerAsync(loginDto.UserName);
        //        return GetToken(user.UserId, user.UserName);
        //    }
        //    else
        //    {
        //        HttpContext.Response.StatusCode = 400;
        //        return "NonUser";
        //    }
        //}

        /// <summary>
        /// 校验用户登录信息
        /// </summary>
        /// <param name="loginDto">登录参数</param>
        /// <returns>登录响应结果</returns>
        [HttpPost]
        public async Task<LoginResponse> CheckLogin(UserLoginDto loginDto)
        {
            // 基础参数校验（不再返回 400 HTTP 状态码，统一 200 + 业务码）
            if (string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.PassWord))
            {
                return new LoginResponse
                {
                    Code = 400,
                    Msg = "用户名或密码不能为空",
                    Data = "",
                };
            }

            // 先判断用户是否存在（支持用户名、手机号、邮箱）
            var user = await _userService.GetCustomerAsync(loginDto.UserName);
            if (user == null)
            {
                return new LoginResponse
                {
                    Code = 400,
                    Msg = "账号不存在，请检查用户名/手机号/邮箱是否正确",
                    Data = "",
                };
            }

            // 再校验密码是否正确
            var inputPwdMd5 = loginDto.PassWord?.ToMd5();
            if (!string.Equals(user.Password, inputPwdMd5, StringComparison.OrdinalIgnoreCase))
            {
                return new LoginResponse
                {
                    Code = 400,
                    Msg = "密码错误",
                    Data = "",
                };
            }

            // 登录成功：更新最后登录时间与IP
            var ip = GetClientIp(HttpContext);
            try
            {
                await _userService.UpdateLastLoginAsync(user.UserId, ip);
            }
            catch { /* 忽略更新异常以不影响登录 */ }

            var token = GetToken(user.UserId, user.UserName);
            
            // 记录登录操作日志（异步执行，不阻塞登录流程）
            // 使用独立的作用域确保服务正确注入
            // 在 Task.Run 之前捕获需要的变量
            var userCode = user.UserName ?? string.Empty;
            var userName = user.NickName ?? user.UserName ?? string.Empty;
            var userId = user.UserId;
            var operationIp = ip ?? "未知";
            var remark = $"姓名：{userName}，工号：{userCode}，登录成功";
            
            _ = Task.Run(async () =>
            {
                try
                {
                    // 使用 IServiceProvider 创建作用域
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var operationLogService = scope.ServiceProvider.GetRequiredService<IOperationLogService>();
                        
                        var logDto = new OperationLogAddDto
                        {
                            UserCode = userCode,
                            UserName = userName,
                            OperationType = OperationType.LOGIN, // 登录操作类型
                            OperationModule = "系统登录",
                            TargetId = userId.ToString(),
                            BeforeData = null,
                            AfterData = null,
                            OperationIp = operationIp,
                            OperationRemark = remark,
                            OperationStatus = "SUCCESS"
                        };
                        
                        await operationLogService.AddOperationLogAsync(logDto);
                    }
                }
                catch (Exception ex)
                {
                    // 记录日志失败不影响登录流程，静默处理
                    // 可以在这里添加日志记录，但为了不影响登录，暂时静默处理
                }
            });
            
            return new LoginResponse
            {
                Code = 200,
                Msg = "登录成功",
                Data = "",
                Token = token,
                User = user,
                Permissions = new List<string>(),
                Roles = new List<string>(),
            };
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns>当前用户信息</returns>
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {           
            long userId = GetUserId();
            if (userId==0)
            {
                return Unauthorized(new { success = false, errorMessage = "用户未登录" });
            }

            // 调用服务层获取用户信息
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, errorMessage = "用户不存在" });
            }

            // 返回前端所需格式的数据
            return Ok(new
            {
                success = true,
                data = new
                {
                    id = user.UserId,
                    name = user.UserName,
                    userName = user.UserName,
                    nickName = user.NickName,
                    avatar = user.Avatar,
                    // 根据前端API.CurrentUser接口添加其他字段
                    roles = user.Roles
                }
            });
        }

        /// <summary>
        /// 退出登录接口
        /// </summary>
        [HttpPost]
        [Authorize] // 确保只有认证用户可访问
        public IActionResult OutLogin()
        {
            try
            {
                // 1. 清除认证Cookie（如果使用Cookie认证）
                Response.Cookies.Delete(".AspNetCore.Cookies");

                // 2. JWT令牌处理（添加到黑名单或设置过期）
                // 如需实现令牌黑名单，可在此处将当前令牌存入分布式缓存（如Redis）
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                // _cache.SetString(token, "invalid", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });

                return Ok(new { success = true, message = "登出成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "登出失败: " + ex.Message });
            }
        }


        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// <summary>
        /// 根据用户信息生成访问令牌
        /// </summary>
        /// <param name="id">用户标识</param>
        /// <param name="userName">用户名</param>
        /// <returns>生成的 JWT 令牌</returns>
        private string GetToken(long id, string userName)
        {
            var token = _configuration.GetSection("Jwt").Get<JwtTokenModel>();
            token.Id = id;
            token.UserName = userName;
            return TokenHelper.CreateToken(token);
        }

        /// <summary>
        /// 获取客户端IP地址
        /// 优先从代理头获取，处理多层代理的情况
        /// </summary>
        private string GetClientIp(HttpContext httpContext)
        {
            var request = httpContext.Request;
            
            // 1. 优先从 X-Forwarded-For 获取（处理多层代理）
            // X-Forwarded-For 格式：client_ip, proxy1_ip, proxy2_ip
            // 第一个IP通常是真实客户端IP
            var xForwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xForwardedFor))
            {
                // 取第一个IP（真实客户端IP）
                var ips = xForwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    var clientIp = ips[0].Trim();
                    // 验证IP格式（简单验证）
                    if (IsValidIpAddress(clientIp))
                    {
                        return clientIp;
                    }
                }
            }
            
            // 2. 从 X-Real-IP 获取（部分反向代理使用）
            var xRealIp = request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xRealIp))
            {
                var realIp = xRealIp.Trim();
                if (IsValidIpAddress(realIp))
                {
                    return realIp;
                }
            }
            
            // 3. 从 CF-Connecting-IP 获取（Cloudflare）
            var cfConnectingIp = request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(cfConnectingIp))
            {
                var cfIp = cfConnectingIp.Trim();
                if (IsValidIpAddress(cfIp))
                {
                    return cfIp;
                }
            }
            
            // 4. 从直接连接获取
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            if (remoteIp != null)
            {
                // 如果是 IPv4 映射的 IPv6 地址，转换为 IPv4
                if (remoteIp.IsIPv4MappedToIPv6)
                {
                    return remoteIp.MapToIPv4().ToString();
                }
                return remoteIp.ToString();
            }
            
            return "未知";
        }
        
        /// <summary>
        /// 验证IP地址格式（简单验证）
        /// </summary>
        private bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return false;
            }
            
            // 简单格式验证：包含点号（IPv4）或冒号（IPv6）
            return ip.Contains('.') || ip.Contains(':');
        }
    }
}
