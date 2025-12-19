using ChargePadLine.WebApi.Controllers.util;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ChargePadLine.WebApi.Controllers
{
    /// <summary>
    /// 豆包AI代理控制器
    /// 用于安全地调用豆包AI API，避免在前端暴露API Key
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DoubaoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DoubaoController> _logger;

        public DoubaoController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<DoubaoController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 豆包AI聊天接口（支持流式和非流式）
        /// </summary>
        /// <param name="request">聊天请求参数</param>
        /// <returns>AI回复（流式或非流式）</returns>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] DoubaoChatRequest request)
        {
            // 记录请求开始
            _logger.LogInformation("收到豆包AI聊天请求: Model={Model}, Stream={Stream}, MessagesCount={MessagesCount}", 
                request?.Model, request?.Stream, request?.Messages?.Count ?? 0);

            try
            {
                // 验证请求参数
                if (request == null)
                {
                    _logger.LogError("请求参数为空");
                    return BadRequest(new ApiResponse
                    {
                        Code = 400,
                        Msg = "请求参数不能为空",
                        Data = null
                    });
                }

                // 从配置中获取API Key（优先从环境变量读取）
                var apiKey = _configuration["DoubaoAI:ApiKey"] 
                    ?? Environment.GetEnvironmentVariable("DoubaoAI__ApiKey");
                
                // 记录配置信息（不记录完整的API Key，只记录是否配置）
                var apiKeyConfigured = !string.IsNullOrEmpty(apiKey);
                _logger.LogInformation("豆包AI配置检查: ApiKey配置={ApiKeyConfigured}, ApiKey长度={ApiKeyLength}", 
                    apiKeyConfigured, apiKey?.Length ?? 0);
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("豆包AI API Key未配置。请检查appsettings.json中的DoubaoAI:ApiKey配置项");
                    return BadRequest(new ApiResponse
                    {
                        Code = 400,
                        Msg = "豆包AI API Key未配置，请联系管理员",
                        Data = null
                    });
                }

                // 获取基础URL（默认值）
                var baseUrl = _configuration["DoubaoAI:BaseUrl"] 
                    ?? "https://ark.cn-beijing.volces.com/api/v3";
                
                _logger.LogInformation("豆包AI配置: BaseUrl={BaseUrl}", baseUrl);

                // 创建HttpClient（优先使用配置好的命名客户端，如果没有则使用默认客户端）
                HttpClient client;
                try
                {
                    client = _httpClientFactory.CreateClient("DoubaoAI");
                    _logger.LogInformation("使用配置的DoubaoAI HttpClient");
                }
                catch (InvalidOperationException)
                {
                    // 如果命名客户端不存在，使用默认客户端
                    client = _httpClientFactory.CreateClient();
                    _logger.LogInformation("使用默认HttpClient（DoubaoAI命名客户端未配置）");
                }
                
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                client.Timeout = TimeSpan.FromMinutes(5); // 设置超时时间为5分钟

                // 1) 兜底模型ID：优先前端传参，其次配置，最后默认值
                var model = string.IsNullOrWhiteSpace(request.Model)
                    ? _configuration["DoubaoAI:DefaultModel"] ?? "doubao-seed-1-6-251015"
                    : request.Model;
                
                _logger.LogInformation("使用的模型: Model={Model}", model);

                // 2) 系统提示词（项目背景），用于让模型理解当前项目
                var systemPrompt =
                "你是“豆包”，精研科技博研IoT管理系统的企业管理员服务助手。"
                + "项目背景：工业IoT管理与产线设备监控平台，核心功能包含设备监控、产线管理、信息追溯、系统设置、日志记录。"
                + "对话策略："
                + "1）上下文记忆：结合本轮对话的历史消息，延续回答，必要时简要引用先前的关键信息，避免重复与自相矛盾。"
                + "2）名称回复：当被问“你叫什么”或类似问题时，自然回复“我叫豆包，是精研科技博研IoT管理系统的智能助手~”，不用机械重复口号。"
                + "3）语气风格：以企业管理员服务助手的身份交流，专业、亲切、简洁，避免网络口语和夸张语气。"
                + "4）项目相关问题：优先结合项目背景给出准确、可执行的说明；"
                + "5）非项目问题：正常回答，不要直接拒绝；"
                + "6）天气类问题：若已提供实时天气数据，直接给出当前天气状况、气温范围、风力等，并附上观测时间与数据来源；"
                + "   若数据超过设定时效（如10-15分钟），先提醒“数据可能已过期”；"
                + "   若未提供实时天气数据且无法联网获取，可简要告知暂未获取到实时天气，并询问是否更新或补充城市信息；"
                + "   若用户未指明城市且你没有上下文城市，请先澄清城市后再回答。"
                + "7）不要编造未提供的功能或天气数据。";

                // 3) 组合消息：在用户消息前添加 system 消息
                var mergedMessages = new List<object>
                {
                    new
                    {
                        role = "system",
                        content = systemPrompt
                    }
                };

                if (request.Messages != null && request.Messages.Count > 0)
                {
                    mergedMessages.AddRange(
                        request.Messages.Select(m => new
                        {
                            role = m.Role,
                            content = m.Content
                        })
                    );
                }

                // 构建请求体
                var requestBody = new
                {
                    model = model,
                    messages = mergedMessages,
                    stream = request.Stream ?? false,
                    temperature = request.Temperature ?? 0.7,
                    max_tokens = request.MaxTokens ?? 2000,
                    top_p = request.TopP,
                    frequency_penalty = request.FrequencyPenalty,
                    presence_penalty = request.PresencePenalty
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var requestUrl = $"{baseUrl}/chat/completions";
                _logger.LogInformation("准备调用豆包AI API: Url={Url}, Model={Model}, Stream={Stream}, RequestBodyLength={RequestBodyLength}", 
                    requestUrl, model, request.Stream, jsonContent.Length);

                // 发送请求到豆包AI API
                HttpResponseMessage? response = null;
                try
                {
                    response = await client.PostAsync(requestUrl, content);
                    _logger.LogInformation("豆包AI API响应: StatusCode={StatusCode}", response.StatusCode);
                }
                catch (Exception httpEx)
                {
                    _logger.LogError(httpEx, "发送HTTP请求时发生异常: Url={Url}, ExceptionType={ExceptionType}, Message={Message}, InnerException={InnerException}", 
                        requestUrl, httpEx.GetType().Name, httpEx.Message, httpEx.InnerException?.Message);
                    throw;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("豆包AI API调用失败: StatusCode={StatusCode}, StatusCodeNumber={StatusCodeNumber}, ErrorContent={ErrorContent}, Url={Url}", 
                        response.StatusCode, (int)response.StatusCode, errorContent, requestUrl);
                    
                    // 根据状态码返回不同的错误信息
                    var errorMessage = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => "API Key无效或已过期，请检查配置",
                        System.Net.HttpStatusCode.TooManyRequests => "请求过于频繁，请稍后再试",
                        System.Net.HttpStatusCode.BadRequest => $"请求参数错误: {errorContent}",
                        System.Net.HttpStatusCode.InternalServerError => "豆包AI服务异常，请稍后重试",
                        System.Net.HttpStatusCode.NotFound => $"API端点不存在: {requestUrl}",
                        System.Net.HttpStatusCode.ServiceUnavailable => "豆包AI服务暂时不可用",
                        _ => $"豆包AI API调用失败: {response.StatusCode}"
                    };

                    return StatusCode((int)response.StatusCode, new ApiResponse
                    {
                        Code = (int)response.StatusCode,
                        Msg = errorMessage,
                        Data = new { ErrorDetail = errorContent }
                    });
                }

                // 如果是流式返回
                if (request.Stream == true)
                {
                    Response.ContentType = "text/event-stream";
                    Response.Headers.Add("Cache-Control", "no-cache");
                    Response.Headers.Add("Connection", "keep-alive");
                    Response.Headers.Add("X-Accel-Buffering", "no"); // 禁用Nginx缓冲

                    try
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        await stream.CopyToAsync(Response.Body, HttpContext.RequestAborted);
                        return new EmptyResult();
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("客户端取消了流式请求");
                        return new EmptyResult();
                    }
                }

                // 非流式返回
                var result = await response.Content.ReadAsStringAsync();
                var jsonResult = JsonSerializer.Deserialize<object>(result);
                
                return Ok(new ApiResponse
                {
                    Code = 200,
                    Msg = "请求成功",
                    Data = jsonResult
                });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "请求超时: ExceptionType={ExceptionType}, Message={Message}, BaseUrl={BaseUrl}", 
                    ex.GetType().Name, ex.Message, _configuration["DoubaoAI:BaseUrl"]);
                return StatusCode(408, new ApiResponse
                {
                    Code = 408,
                    Msg = "请求超时，请稍后重试。可能是网络连接问题或服务器响应慢",
                    Data = new { ErrorType = "Timeout", Message = ex.Message }
                });
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                // 详细的HTTP请求异常处理
                var errorType = "NetworkError";
                var errorMsg = "网络请求失败";
                
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is System.IO.IOException)
                    {
                        errorType = "SSLConnectionError";
                        errorMsg = "SSL/TLS连接失败";
                        _logger.LogError(ex, "SSL/TLS连接失败: Message={Message}, InnerException={InnerException}, BaseUrl={BaseUrl}", 
                            ex.Message, ex.InnerException.Message, _configuration["DoubaoAI:BaseUrl"]);
                    }
                    else if (ex.Message.Contains("DNS") || ex.Message.Contains("主机") || ex.Message.Contains("host"))
                    {
                        errorType = "DnsResolutionError";
                        errorMsg = "DNS解析失败，无法连接到服务器";
                        _logger.LogError(ex, "DNS解析失败: Message={Message}, InnerException={InnerException}, BaseUrl={BaseUrl}", 
                            ex.Message, ex.InnerException.Message, _configuration["DoubaoAI:BaseUrl"]);
                    }
                    else
                    {
                        _logger.LogError(ex, "HTTP请求异常: Message={Message}, InnerException={InnerException}, BaseUrl={BaseUrl}", 
                            ex.Message, ex.InnerException.Message, _configuration["DoubaoAI:BaseUrl"]);
                    }
                }
                else
                {
                    _logger.LogError(ex, "HTTP请求异常: Message={Message}, BaseUrl={BaseUrl}, StackTrace={StackTrace}", 
                        ex.Message, _configuration["DoubaoAI:BaseUrl"], ex.StackTrace);
                }
                
                return StatusCode(500, new ApiResponse
                {
                    Code = 500,
                    Msg = $"{errorMsg}，请检查网络连接或联系管理员",
                    Data = new 
                    { 
                        ErrorType = errorType, 
                        Message = ex.Message,
                        InnerException = ex.InnerException?.Message,
                        BaseUrl = _configuration["DoubaoAI:BaseUrl"]
                    }
                });
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "JSON序列化/反序列化错误: Message={Message}, Path={Path}", 
                    ex.Message, ex.Path);
                return StatusCode(500, new ApiResponse
                {
                    Code = 500,
                    Msg = "数据格式错误",
                    Data = new { ErrorType = "JsonError", Message = ex.Message }
                });
            }
            catch (Exception ex)
            {
                // 记录完整的异常信息
                _logger.LogError(ex, "调用豆包AI接口时发生未预期错误: ExceptionType={ExceptionType}, Message={Message}, StackTrace={StackTrace}, BaseUrl={BaseUrl}, ApiKeyConfigured={ApiKeyConfigured}", 
                    ex.GetType().Name, 
                    ex.Message, 
                    ex.StackTrace, 
                    _configuration["DoubaoAI:BaseUrl"],
                    !string.IsNullOrEmpty(_configuration["DoubaoAI:ApiKey"]));
                
                // 记录内部异常
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "内部异常: ExceptionType={ExceptionType}, Message={Message}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                return StatusCode(500, new ApiResponse
                {
                    Code = 500,
                    Msg = "服务器内部错误，请稍后重试。详细错误信息已记录到日志",
                    Data = new 
                    { 
                        ErrorType = ex.GetType().Name, 
                        Message = ex.Message,
                        InnerException = ex.InnerException?.Message
                    }
                });
            }
        }
    }

    /// <summary>
    /// 豆包AI聊天请求模型
    /// </summary>
    public class DoubaoChatRequest
    {
        /// <summary>
        /// 模型端点ID
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// 消息列表
        /// </summary>
        public List<ChatMessage> Messages { get; set; } = new();

        /// <summary>
        /// 是否启用流式返回
        /// </summary>
        public bool? Stream { get; set; }

        /// <summary>
        /// 温度参数（0-2，控制随机性）
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// 最大生成token数
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Top-p采样参数
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// 频率惩罚
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// 存在惩罚
        /// </summary>
        public double? PresencePenalty { get; set; }
    }

    /// <summary>
    /// 聊天消息模型
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// 角色：user, assistant, system
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}

