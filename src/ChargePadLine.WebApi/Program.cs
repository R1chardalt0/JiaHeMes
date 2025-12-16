using ChargePadLine.WebApi;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using ChargePadLine.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ChargePadLine.Common.TokenModule.Models;
using System.Reflection;
using ChargePadLine.Service;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = builder.Configuration;
// 读取 appsettings.json 中的 Urls 配置
var urls = builder.Configuration["Urls"] ?? "http://0.0.0.0:5000";
// 设置应用监听的 URL（覆盖默认配置）
builder.WebHost.UseUrls(urls);

#region Jwt验证
var token = config.GetSection("Jwt").Get<JwtTokenModel>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opt =>
{
    //是否是https,默认是true
    opt.RequireHttpsMetadata = false;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new()
    {
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(token.Security)),
        ValidIssuer = token.Issuer,
        ValidAudience = token.Audience,
    };
    opt.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {  //此处终止代码
            context.HandleResponse();
            var res = "{\"code\":203,\"err\":\"权限验证失败\"}";
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status203NonAuthoritative;
            await context.Response.WriteAsync(res);
            await Task.FromResult(0);
        }
    };
});
#endregion

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

InitializeDatabaseAndStartServices(app.Services);

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowClient"); // 新增：启用 CORS


var logger = app.Services.GetRequiredService<ILogger<Program>>();

// 先映射控制器（包括 StaticFilesController），确保控制器路由优先
app.MapControllers();

app.UseHttpsRedirection();
app.UseAuthentication(); // 在UseAuthorization前添加
app.UseAuthorization();
app.Run();

#region 配置服务
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{

    // 添加控制器
    services.AddControllers(options =>
    {
        // 注册操作日志 ActionFilter（全局生效）
        options.Filters.Add<ChargePadLine.WebApi.Filters.OperationLogActionFilter>();
    });
    services.AddEndpointsApiExplorer();
    // 豆包AI代理调用需要 HttpClient 工厂
    services.AddHttpClient();
    // 注册数据库和业务服务
    services.AddDBServices(configuration);
    services.AddBusinessServices(configuration);

    // 添加 CORS 策略（允许所有客户端，生产环境需限制具体源）
    // 服务端 CORS 策略（示例）
    services.AddCors(options =>
    {
        options.AddPolicy("AllowClient", policy =>
        {
            policy.WithOrigins()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
}
#endregion

#region 数据库服务
void InitializeDatabaseAndStartServices(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var scopedServices = scope.ServiceProvider;
        try
        {
            // 初始化AppDbContext数据库
            var dbContext = scopedServices.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();

            // 初始化ReportDbContext数据库
            var reportDbContext = scopedServices.GetRequiredService<ReportDbContext>();
            reportDbContext.Database.EnsureCreated();
            var logger = scopedServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("数据库创建成功");
        }
        catch (Exception ex)
        {
            var logger = scopedServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "初始化过程中发生错误");
            throw;
        }
    }
}
#endregion