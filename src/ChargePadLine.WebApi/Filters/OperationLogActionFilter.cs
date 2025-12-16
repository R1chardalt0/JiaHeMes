using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.OperationLog;
using ChargePadLine.Service.OperationLog.Dto;
using ChargePadLine.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Filters
{
    /// <summary>
    /// 操作日志 Action 过滤器
    /// 自动拦截增删改接口，记录操作日志
    /// </summary>
    public class OperationLogActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<OperationLogActionFilter> _logger;
        private object? _beforeDataCache = null;

        public OperationLogActionFilter(ILogger<OperationLogActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 在执行 Action 之前，先获取操作前数据（用于 UPDATE 和 DELETE 操作）
            var httpMethod = context.HttpContext.Request.Method.ToUpper();
            var operationType = GetOperationType(httpMethod, context.RouteData.Values["action"]?.ToString());
            
            // 对于 UPDATE 和 DELETE 操作，在执行前查询数据库获取原始数据
            if (operationType == OperationType.UPDATE || operationType == OperationType.DELETE)
            {
                _beforeDataCache = await GetBeforeDataFromDatabase(context, operationType);
            }

            // 执行 Action
            var executedContext = await next();

            // 只记录增删改操作（POST、PUT、DELETE），不记录查询操作（GET）
            if (httpMethod != "POST" && httpMethod != "PUT" && httpMethod != "DELETE")
            {
                return;
            }

            // 跳过操作日志相关的接口，避免递归记录
            var controllerName = context.RouteData.Values["controller"]?.ToString()?.ToLower();
            var actionName = context.RouteData.Values["action"]?.ToString()?.ToLower();
            
            // 跳过系统接口，不记录操作日志（登录、登出等）
            // 这些是系统操作，不是业务数据操作，不应该记录
            if (controllerName == "sysoperationlog" || controllerName == "operationlog" ||
                controllerName == "login" || 
                (actionName?.Contains("login") ?? false) ||
                (actionName?.Contains("logout") ?? false) ||
                (actionName?.Contains("outlogin") ?? false) ||
                (actionName?.Contains("checklogin") ?? false))
            {
                _logger.LogDebug("跳过系统接口操作日志记录：Controller={Controller}, Action={Action}", 
                    controllerName, actionName);
                return;
            }
            
            // 跳过信息追溯模块的接口，这些是自动化数据采集，不是用户手动操作
            // 设备信息追溯、产品信息追溯、产量报表
            // 注意：有些控制器使用了 [Route("api/[action]")]，可能 controller 名称为空，需要通过 action 名称判断
            var isTraceModule = 
                controllerName == "deviceinfocollection" || 
                controllerName == "productinfocollection" ||
                controllerName == "producttraceinfo" ||
                controllerName == "deviceinfo" ||
                controllerName == "productionline" ||
                controllerName == "equmenttraceinfocollection" ||
                (actionName?.Contains("deviceinfocollection") ?? false) ||
                (actionName?.Contains("productinfocollection") ?? false) ||
                (actionName?.Contains("producttraceinfo") ?? false) ||
                (actionName?.Contains("devicedatacollection") ?? false) ||
                (actionName?.Contains("datacollection") ?? false) ||
                (actionName?.Contains("datacollectforsfc") ?? false) ||
                (actionName?.Contains("getproductionrecords") ?? false) ||
                (actionName?.Contains("gethourlyproductionrecords") ?? false);
            
            if (isTraceModule)
            {
                _logger.LogDebug("跳过信息追溯模块操作日志记录：Controller={Controller}, Action={Action}", 
                    controllerName, actionName);
                return;
            }

            try
            {
                // 获取操作日志服务
                var operationLogService = context.HttpContext.RequestServices.GetService<IOperationLogService>();
                if (operationLogService == null)
                {
                    _logger.LogWarning("操作日志服务未注册，无法记录操作日志");
                    return;
                }

                // 获取当前用户信息（优先从JWT token获取，如果未登录则从请求体获取）
                var userId = GetUserId(context);
                var userCode = GetUserCode(context);
                var userName = GetUserName(context);
                
                // 如果已登录（有userId），从数据库获取完整的用户信息（包括姓名）
                if (userId > 0)
                {
                    try
                    {
                        using (var scope = context.HttpContext.RequestServices.CreateScope())
                        {
                            var userService = scope.ServiceProvider.GetService<ChargePadLine.Service.Systems.IUserService>();
                            if (userService != null)
                            {
                                var user = await userService.GetUserByIdAsync(userId);
                                if (user != null)
                                {
                                    // UserName 是工号，NickName 是姓名
                                    userCode = user.UserName ?? userCode;
                                    userName = user.NickName ?? user.UserName ?? userName;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "从数据库获取用户信息失败，使用token中的信息: {Error}", ex.Message);
                    }
                }
                
                // 如果未登录，尝试从请求体中获取用户信息（用于注册等场景）
                if (userId == 0 || string.IsNullOrWhiteSpace(userCode))
                {
                    var userInfoFromRequest = GetUserInfoFromRequest(context);
                    if (userInfoFromRequest.HasValue)
                    {
                        userCode = userInfoFromRequest.Value.UserCode;
                        userName = userInfoFromRequest.Value.UserName ?? userCode;
                    }
                    else
                    {
                        // 如果既没有登录信息，也无法从请求体获取，则使用默认值
                        userCode = "系统";
                        userName = "系统";
                    }
                }

                // 操作类型已在方法开始时获取，直接使用

                // 获取操作模块（从 Controller 名称推断）
                var operationModule = GetOperationModule(controllerName);

                // 获取操作对象ID（从路由参数或请求体获取）
                var targetId = GetTargetId(context, executedContext);

                // 获取操作前后数据
                var beforeData = GetBeforeData(context, executedContext, _beforeDataCache);
                var afterData = GetAfterData(context, executedContext);

                // 获取操作IP
                var operationIp = GetClientIp(context);

                // 获取操作状态
                var operationStatus = executedContext.Exception == null ? OperationStatus.SUCCESS : OperationStatus.FAIL;

                // 构建操作备注
                var operationRemark = GetOperationRemark(context, executedContext);
                
                // 确保操作备注不为空
                if (string.IsNullOrWhiteSpace(operationRemark))
                {
                    var module = GetOperationModule(controllerName);
                    var operationTypeName = operationType == OperationType.INSERT ? "新增" :
                                           operationType == OperationType.UPDATE ? "修改" : "删除";
                    operationRemark = $"{userName ?? "未知用户"}(工号:{userCode}) 在{module}中 {operationTypeName}了数据";
                }

                // 创建操作日志DTO
                var logDto = new OperationLogAddDto
                {
                    UserCode = userCode,
                    UserName = userName ?? "未知用户",
                    OperationType = operationType,
                    OperationModule = operationModule,
                    TargetId = targetId,
                    BeforeData = beforeData,
                    AfterData = afterData,
                    OperationIp = operationIp,
                    OperationRemark = operationRemark,
                    OperationStatus = operationStatus
                };

                // 记录调试日志
                _logger.LogInformation("准备记录操作日志：用户={UserCode}, 模块={Module}, 操作={Operation}, 对象ID={TargetId}",
                    userCode, operationModule, operationType, targetId);

                // 在 HTTP 上下文还存在时获取 IServiceScopeFactory，避免后续访问时上下文已被释放
                var serviceScopeFactory = context.HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                
                // 记录操作日志（使用后台任务，不阻塞主业务流程）
                // 注意：使用 Task.Run 确保在后台线程执行，避免阻塞请求
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // 使用独立的服务作用域，确保服务正确注入
                        // 使用之前保存的 serviceScopeFactory，而不是直接访问 context.HttpContext
                        using (var scope = serviceScopeFactory.CreateScope())
                        {
                            var scopedLogService = scope.ServiceProvider.GetRequiredService<IOperationLogService>();
                            await scopedLogService.AddOperationLogAsync(logDto);
                            _logger.LogInformation("操作日志记录成功：用户={UserCode}, 模块={Module}, 操作={Operation}",
                                userCode, operationModule, operationType);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "异步记录操作日志失败：用户={UserCode}, 模块={Module}, 操作={Operation}, 错误={Error}",
                            userCode, operationModule, operationType, ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                // 记录日志失败不影响主业务流程
                _logger.LogWarning(ex, "操作日志记录异常");
            }
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        private long GetUserId(ActionExecutingContext context)
        {
            var claimVal = context.HttpContext.User.FindFirst("id")?.Value ?? 
                          context.HttpContext.User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(claimVal)) return 0;
            return long.TryParse(claimVal, out var id) ? id : 0;
        }

        /// <summary>
        /// 获取用户工号
        /// </summary>
        private string GetUserCode(ActionExecutingContext context)
        {
            return context.HttpContext.User.FindFirst("userName")?.Value ??
                   context.HttpContext.User.FindFirst("UserName")?.Value ??
                   context.HttpContext.User.FindFirst("user_code")?.Value ??
                   context.HttpContext.User.FindFirst("UserCode")?.Value ??
                   string.Empty;
        }

        /// <summary>
        /// 获取用户姓名
        /// </summary>
        private string? GetUserName(ActionExecutingContext context)
        {
            return context.HttpContext.User.FindFirst("nickName")?.Value ??
                   context.HttpContext.User.FindFirst("NickName")?.Value ??
                   context.HttpContext.User.FindFirst("user_name")?.Value ??
                   context.HttpContext.User.FindFirst("UserName")?.Value;
        }

        /// <summary>
        /// 从请求体中获取用户信息（用于未登录场景，如注册）
        /// </summary>
        private (string UserCode, string UserName)? GetUserInfoFromRequest(ActionExecutingContext context)
        {
            if (context.ActionArguments.Count == 0)
            {
                return null;
            }

            try
            {
                // 遍历所有参数，查找包含用户信息的对象
                foreach (var arg in context.ActionArguments.Values)
                {
                    if (arg == null) continue;

                    var type = arg.GetType();
                    
                    // 尝试获取 UserName 或 userCode 字段（作为工号）
                    var userCodeProp = type.GetProperty("UserName") ?? 
                                      type.GetProperty("userName") ??
                                      type.GetProperty("UserCode") ??
                                      type.GetProperty("userCode");
                    
                    // 尝试获取 NickName 或 name 字段（作为姓名）
                    var userNameProp = type.GetProperty("NickName") ?? 
                                      type.GetProperty("nickName") ??
                                      type.GetProperty("Name") ??
                                      type.GetProperty("name");

                    if (userCodeProp != null)
                    {
                        var userCode = userCodeProp.GetValue(arg)?.ToString();
                        var userName = userNameProp?.GetValue(arg)?.ToString() ?? userCode;
                        
                        if (!string.IsNullOrWhiteSpace(userCode))
                        {
                            return (userCode, userName ?? userCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "从请求体获取用户信息失败: {Error}", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 获取操作类型
        /// </summary>
        private string GetOperationType(string httpMethod, string? actionName)
        {
            // 将 actionName 转换为小写以便进行大小写不敏感的匹配
            var actionNameLower = actionName?.ToLower() ?? string.Empty;
            
            // 优先根据 Action 名称判断操作类型（更准确）
            // 1. 优先检查 DELETE：action 名称包含 "delete"
            if (actionNameLower.Contains("delete"))
            {
                return OperationType.DELETE;
            }
            
            // 2. 检查 UPDATE：action 名称包含 "update" 或 "edit"
            if (actionNameLower.Contains("update") || actionNameLower.Contains("edit"))
            {
                return OperationType.UPDATE;
            }
            
            // 3. 检查 INSERT：action 名称包含 "add" 或 "create"
            if (actionNameLower.Contains("add") || actionNameLower.Contains("create"))
            {
                return OperationType.INSERT;
            }
            
            // 如果 Action 名称无法判断，则根据 HTTP 方法判断
            // DELETE 方法 -> DELETE
            if (httpMethod == "DELETE")
            {
                return OperationType.DELETE;
            }
            
            // PUT 方法 -> UPDATE
            if (httpMethod == "PUT")
            {
                return OperationType.UPDATE;
            }
            
            // POST 方法 -> 默认 INSERT（但如果没有明确的标识，可能是其他操作）
            // 注意：这里默认返回 INSERT，因为新增操作通常使用 POST
            if (httpMethod == "POST")
            {
                return OperationType.INSERT;
            }
            
            // 其他方法（如 GET、PATCH 等）默认返回 UPDATE
            return OperationType.UPDATE;
        }

        /// <summary>
        /// 获取操作模块
        /// </summary>
        private string GetOperationModule(string? controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                return "未知模块";
            }

            // 将 Controller 名称转换为中文模块名
            var moduleMap = new Dictionary<string, string>
            {
                { "sysuser", "用户管理" },
                { "sysrole", "角色管理" },
                { "sysmenu", "菜单管理" },
                { "sysdept", "部门管理" },
                { "syspost", "岗位管理" },
                { "syscompany", "公司管理" },
                { "deviceinfo", "设备管理" },
                { "productionline", "产线管理" },
                { "producttraceinfo", "产品追溯" },
                { "equipmenttracinfo", "设备追溯" }
            };

            return moduleMap.TryGetValue(controllerName.ToLower(), out var module) 
                ? module 
                : $"{controllerName}管理";
        }

        /// <summary>
        /// 获取操作对象ID
        /// </summary>
        private string GetTargetId(ActionExecutingContext context, ActionExecutedContext executedContext)
        {
            // 优先从路由参数获取
            var routeValues = context.RouteData.Values;
            foreach (var key in new[] { "id", "Id", "userId", "UserId", "targetId", "TargetId" })
            {
                if (routeValues.TryGetValue(key, out var value) && value != null)
                {
                    var idStr = value.ToString();
                    if (!string.IsNullOrWhiteSpace(idStr))
                    {
                        return idStr;
                    }
                }
            }

            // 从请求体获取
            if (context.ActionArguments.Count > 0)
            {
                // 遍历所有参数，查找 ID
                foreach (var arg in context.ActionArguments.Values)
                {
                    if (arg == null) continue;
                    
                    // 如果是数组（批量删除），返回所有ID
                    if (arg is long[] ids)
                    {
                        return ids.Length > 0 ? string.Join(",", ids) : "未知";
                    }
                    
                    // 如果是单个 long 类型
                    if (arg is long singleId)
                    {
                        return singleId.ToString();
                    }
                    
                    // 如果是单个 int 类型
                    if (arg is int intId)
                    {
                        return intId.ToString();
                    }
                    
                    // 尝试从对象属性获取ID
                    var type = arg.GetType();
                    var idProperty = type.GetProperty("Id") ?? 
                                    type.GetProperty("id") ??
                                    type.GetProperty("UserId") ??
                                    type.GetProperty("userId") ??
                                    type.GetProperty("CompanyId") ??
                                    type.GetProperty("companyId") ??
                                    type.GetProperty("DeptId") ??
                                    type.GetProperty("deptId");
                    if (idProperty != null)
                    {
                        var idValue = idProperty.GetValue(arg);
                        if (idValue != null)
                        {
                            return idValue.ToString() ?? "未知";
                        }
                    }
                }
            }

            return "未知";
        }

        /// <summary>
        /// 从数据库查询操作前数据（用于 UPDATE 和 DELETE 操作）
        /// </summary>
        private async Task<object?> GetBeforeDataFromDatabase(ActionExecutingContext context, string operationType)
        {
            try
            {
                // 获取目标ID
                var targetId = GetTargetIdFromRequest(context);
                if (string.IsNullOrWhiteSpace(targetId) || targetId == "未知")
                {
                    return null;
                }

                var controllerName = context.RouteData.Values["controller"]?.ToString()?.ToLower();
                
                // 根据不同的控制器，使用不同的服务查询数据
                using (var scope = context.HttpContext.RequestServices.CreateScope())
                {
                    // 用户管理模块
                    if (controllerName == "sysuser")
                    {
                        var userService = scope.ServiceProvider.GetService<ChargePadLine.Service.Systems.IUserService>();
                        if (userService != null)
                        {
                            // 检查是否是批量删除（ID数组）
                            if (context.ActionArguments.Count > 0)
                            {
                                var firstArg = context.ActionArguments.Values.FirstOrDefault();
                                if (firstArg is long[] ids && ids.Length > 0)
                                {
                                    // 批量删除：查询所有被删除的用户
                                    var users = new List<object>();
                                    foreach (var id in ids)
                                    {
                                        try
                                        {
                                            var user = await userService.GetUserByIdAsync(id);
                                            if (user != null)
                                            {
                                                users.Add(RemoveSensitiveFields(user));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogWarning(ex, "查询用户数据失败，ID={UserId}", id);
                                        }
                                    }
                                    if (users.Count > 0)
                                    {
                                        return new { 
                                            Ids = ids, 
                                            Count = ids.Length,
                                            Users = users,
                                            Operation = "批量删除"
                                        };
                                    }
                                    // 如果查询失败，至少记录ID
                                    return new { 
                                        Ids = ids, 
                                        Count = ids.Length,
                                        Operation = "批量删除",
                                        Note = "无法从数据库获取被删除的数据"
                                    };
                                }
                            }
                            
                            // 单个删除或修改：尝试解析ID
                            if (long.TryParse(targetId, out var userId))
                            {
                                try
                                {
                                    var user = await userService.GetUserByIdAsync(userId);
                                    if (user != null)
                                    {
                                        return RemoveSensitiveFields(user);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "查询用户数据失败，ID={UserId}", userId);
                                }
                            }
                        }
                    }
                    
                    // 可以在这里添加其他模块的查询逻辑
                    // 例如：角色管理、部门管理等
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "从数据库查询操作前数据失败: {Error}", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 从请求中获取目标ID（用于查询数据库）
        /// </summary>
        private string GetTargetIdFromRequest(ActionExecutingContext context)
        {
            // 优先从路由参数获取
            var routeValues = context.RouteData.Values;
            foreach (var key in new[] { "id", "Id", "userId", "UserId", "targetId", "TargetId" })
            {
                if (routeValues.TryGetValue(key, out var value) && value != null)
                {
                    return value.ToString() ?? "未知";
                }
            }

            // 从请求体获取
            if (context.ActionArguments.Count > 0)
            {
                var firstArg = context.ActionArguments.Values.FirstOrDefault();
                if (firstArg != null)
                {
                    // 如果是数组，返回第一个ID（用于单个查询）
                    if (firstArg is long[] ids && ids.Length > 0)
                    {
                        return ids[0].ToString();
                    }
                    
                    // 尝试从对象属性获取ID
                    var type = firstArg.GetType();
                    var idProperty = type.GetProperty("Id") ?? 
                                    type.GetProperty("id") ??
                                    type.GetProperty("UserId") ??
                                    type.GetProperty("userId");
                    if (idProperty != null)
                    {
                        var idValue = idProperty.GetValue(firstArg);
                        if (idValue != null)
                        {
                            return idValue.ToString() ?? "未知";
                        }
                    }
                }
            }

            return "未知";
        }

        /// <summary>
        /// 获取操作前数据（仅 UPDATE 和 DELETE 操作）
        /// </summary>
        private string? GetBeforeData(ActionExecutingContext context, ActionExecutedContext executedContext, object? beforeDataCache)
        {
            var operationType = GetOperationType(context.HttpContext.Request.Method.ToUpper(), 
                context.RouteData.Values["action"]?.ToString());
            
            // 仅 UPDATE 和 DELETE 操作需要记录操作前数据
            if (operationType != OperationType.UPDATE && operationType != OperationType.DELETE)
            {
                return null;
            }

            try
            {
                var jsonOptions = new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                // 优先使用从数据库查询的数据
                if (beforeDataCache != null)
                {
                    return JsonSerializer.Serialize(beforeDataCache, jsonOptions);
                }
                
                // 如果没有从数据库查询到，尝试从请求参数获取
                if (context.ActionArguments.Count > 0)
                {
                    var firstArg = context.ActionArguments.Values.FirstOrDefault();
                    
                    // 对于删除操作，如果是数组，记录所有ID和数量
                    if (firstArg is long[] ids)
                    {
                        if (ids.Length > 0)
                        {
                            return JsonSerializer.Serialize(new { 
                                Ids = ids, 
                                Count = ids.Length,
                                Operation = "批量删除",
                                Note = "无法从数据库获取被删除的数据"
                            }, jsonOptions);
                        }
                        return null;
                    }
                    
                    // 对于其他操作，序列化请求参数
                    if (firstArg != null)
                    {
                        var data = RemoveSensitiveFields(firstArg);
                        return JsonSerializer.Serialize(data, jsonOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "序列化操作前数据失败: {Error}", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 移除敏感字段（如密码）
        /// </summary>
        private object RemoveSensitiveFields(object obj)
        {
            if (obj == null) return obj;
            
            try
            {
                var type = obj.GetType();
                var properties = type.GetProperties();
                var result = new Dictionary<string, object?>();
                
                foreach (var prop in properties)
                {
                    var propName = prop.Name.ToLower();
                    // 排除密码字段
                    if (propName.Contains("password") || propName.Contains("pwd"))
                    {
                        result[prop.Name] = "***";
                    }
                    else
                    {
                        var value = prop.GetValue(obj);
                        result[prop.Name] = value;
                    }
                }
                
                return result;
            }
            catch
            {
                return obj;
            }
        }

        /// <summary>
        /// 获取操作后数据（仅 INSERT 和 UPDATE 操作）
        /// 注意：保存的是实际的数据对象，而不是响应消息
        /// </summary>
        private string? GetAfterData(ActionExecutingContext context, ActionExecutedContext executedContext)
        {
            var operationType = GetOperationType(context.HttpContext.Request.Method.ToUpper(), 
                context.RouteData.Values["action"]?.ToString());
            
            // 仅 INSERT 和 UPDATE 操作需要记录操作后数据
            if (operationType != OperationType.INSERT && operationType != OperationType.UPDATE)
            {
                return null;
            }

            // 优先从请求参数获取实际数据对象（而不是响应消息）
            if (context.ActionArguments.Count > 0)
            {
                try
                {
                    var jsonOptions = new JsonSerializerOptions 
                    { 
                        WriteIndented = false,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    
                    var firstArg = context.ActionArguments.Values.FirstOrDefault();
                    if (firstArg != null)
                    {
                        // 排除密码等敏感字段
                        var data = RemoveSensitiveFields(firstArg);
                        return JsonSerializer.Serialize(data, jsonOptions);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "序列化请求参数失败: {Error}", ex.Message);
                }
            }

            // 如果请求参数中没有数据，尝试从响应结果中获取（但只取data字段，不取整个响应）
            if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                try
                {
                    var jsonOptions = new JsonSerializerOptions 
                    { 
                        WriteIndented = false,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    
                    var resultType = objectResult.Value.GetType();
                    // 只取data字段，忽略code、msg等响应消息字段
                    var dataProperty = resultType.GetProperty("data") ?? resultType.GetProperty("Data");
                    if (dataProperty != null)
                    {
                        var dataValue = dataProperty.GetValue(objectResult.Value);
                        if (dataValue != null)
                        {
                            var data = RemoveSensitiveFields(dataValue);
                            return JsonSerializer.Serialize(data, jsonOptions);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "序列化响应结果失败: {Error}", ex.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// 获取客户端IP地址
        /// 优先从代理头获取，处理多层代理的情况
        /// </summary>
        private string GetClientIp(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            
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
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
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
            
            // 排除本地回环地址和私有地址（如果这些是代理IP，说明没有真实客户端IP）
            // 但这里不排除，因为可能是内网访问
            
            // 简单格式验证：包含点号（IPv4）或冒号（IPv6）
            return ip.Contains('.') || ip.Contains(':');
        }

        /// <summary>
        /// 获取操作备注
        /// </summary>
        private string? GetOperationRemark(ActionExecutingContext context, ActionExecutedContext executedContext)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();
            var operationType = GetOperationType(context.HttpContext.Request.Method.ToUpper(), actionName);

            // 获取用户信息
            var userId = GetUserId(context);
            var userCode = GetUserCode(context);
            var userName = GetUserName(context);
            
            // 如果未登录，尝试从请求体中获取用户信息
            if (userId == 0 || string.IsNullOrWhiteSpace(userCode))
            {
                var userInfoFromRequest = GetUserInfoFromRequest(context);
                if (userInfoFromRequest.HasValue)
                {
                    userCode = userInfoFromRequest.Value.UserCode;
                    userName = userInfoFromRequest.Value.UserName ?? userCode;
                }
            }

            // 获取操作对象ID
            var targetId = GetTargetId(context, executedContext);

            // 构建操作描述
            var module = GetOperationModule(controllerName?.ToLower());
            var operationTypeName = operationType == OperationType.INSERT ? "新增" :
                                   operationType == OperationType.UPDATE ? "修改" : "删除";

            // 构建详细的操作备注，格式：用户姓名(工号) 在模块中 操作类型 了对象ID的数据
            // 例如：张三(123456) 在用户管理中 删除了 ID为100 的数据
            var remark = $"{userName ?? "未知用户"}";
            if (!string.IsNullOrWhiteSpace(userCode) && userCode != userName)
            {
                remark += $"(工号:{userCode})";
            }
            remark += $" 在{module}中 {operationTypeName}了";
            
            // 尝试获取更详细的操作对象信息
            var targetInfo = GetTargetInfo(context, executedContext, operationType);
            if (!string.IsNullOrWhiteSpace(targetInfo))
            {
                remark += $" {targetInfo}";
            }
            else if (targetId != "未知")
            {
                remark += $" ID为{targetId} 的数据";
            }
            else
            {
                remark += " 数据";
            }

            return remark;
        }

        /// <summary>
        /// 获取操作对象的详细信息（用于构建操作备注）
        /// </summary>
        private string? GetTargetInfo(ActionExecutingContext context, ActionExecutedContext executedContext, string operationType)
        {
            try
            {
                  // 对于删除操作，只从请求参数中获取（通常是ID数组）
                if (operationType == OperationType.DELETE)
                {
                    if (context.ActionArguments.Count > 0)
                    {
                        var firstArg = context.ActionArguments.Values.FirstOrDefault();
                        if (firstArg != null)
                        {
                            // 删除操作通常是ID数组
                            if (firstArg is long[] ids)
                            {
                                if (ids.Length > 1)
                                {
                                    return $"ID为[{string.Join(",", ids)}] 的{ids.Length}条数据";
                                }
                                else if (ids.Length == 1)
                                {
                                    return $"ID为{ids[0]} 的数据";
                                }
                            }
                            
                            // 如果不是数组，尝试获取单个ID
                            var type = firstArg.GetType();
                            var idProp = type.GetProperty("Id") ?? 
                                        type.GetProperty("id") ??
                                        type.GetProperty("UserId") ??
                                        type.GetProperty("userId");
                            var idVal = idProp?.GetValue(firstArg)?.ToString();
                            if (!string.IsNullOrWhiteSpace(idVal))
                            {
                                return $"ID为{idVal} 的数据";
                            }
                        }
                    }
                    
                    // 对于删除操作，也可以从路由参数获取ID
                    var routeValues = context.RouteData.Values;
                    foreach (var key in new[] { "id", "Id", "userId", "UserId" })
                    {
                        if (routeValues.TryGetValue(key, out var value) && value != null)
                        {
                            return $"ID为{value} 的数据";
                        }
                    }
                    
                    return null; // 删除操作不尝试从响应结果获取
                }
                
                // 对于新增和修改操作，优先从请求参数中获取对象信息
                if (context.ActionArguments.Count > 0)
                {
                    var firstArg = context.ActionArguments.Values.FirstOrDefault();
                    if (firstArg != null)
                    {
                        // 尝试从对象中获取名称字段（如UserName, Name, NickName等）
                        var type = firstArg.GetType();
                        var nameProperty = type.GetProperty("UserName") ?? 
                                         type.GetProperty("userName") ??
                                         type.GetProperty("Name") ??
                                         type.GetProperty("name") ??
                                         type.GetProperty("NickName") ??
                                         type.GetProperty("nickName");
                        
                        if (nameProperty != null)
                        {
                            var nameValue = nameProperty.GetValue(firstArg)?.ToString();
                            if (!string.IsNullOrWhiteSpace(nameValue))
                            {
                                // 同时获取ID
                                var idProperty = type.GetProperty("Id") ?? 
                                               type.GetProperty("id") ??
                                               type.GetProperty("UserId") ??
                                               type.GetProperty("userId");
                                var idValue = idProperty?.GetValue(firstArg)?.ToString();
                                
                                if (!string.IsNullOrWhiteSpace(idValue))
                                {
                                    return $"ID为{idValue}、名称为{nameValue} 的数据";
                                }
                                return $"名称为{nameValue} 的数据";
                            }
                        }
                        
                        // 如果无法获取名称，至少获取ID
                        var idProp = type.GetProperty("Id") ?? 
                                    type.GetProperty("id") ??
                                    type.GetProperty("UserId") ??
                                    type.GetProperty("userId");
                        var idVal = idProp?.GetValue(firstArg)?.ToString();
                        if (!string.IsNullOrWhiteSpace(idVal))
                        {
                            return $"ID为{idVal} 的数据";
                        }
                    }
                }
                
                // 对于新增和修改操作，如果请求参数中没有，尝试从响应结果中获取对象信息
                if (operationType == OperationType.INSERT || operationType == OperationType.UPDATE)
                {
                    if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
                    {
                        var resultType = objectResult.Value.GetType();
                        // 尝试获取data字段
                        var dataProperty = resultType.GetProperty("data") ?? resultType.GetProperty("Data");
                        if (dataProperty != null)
                        {
                            var dataValue = dataProperty.GetValue(objectResult.Value);
                            if (dataValue != null)
                            {
                                var dataType = dataValue.GetType();
                                var nameProp = dataType.GetProperty("UserName") ?? 
                                             dataType.GetProperty("userName") ??
                                             dataType.GetProperty("Name") ??
                                             dataType.GetProperty("name") ??
                                             dataType.GetProperty("NickName") ??
                                             dataType.GetProperty("nickName");
                                if (nameProp != null)
                                {
                                    var nameVal = nameProp.GetValue(dataValue)?.ToString();
                                    if (!string.IsNullOrWhiteSpace(nameVal))
                                    {
                                        return $"名称为{nameVal} 的数据";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "获取操作对象详细信息失败: {Error}", ex.Message);
            }

            return null;
        }
    }
}

