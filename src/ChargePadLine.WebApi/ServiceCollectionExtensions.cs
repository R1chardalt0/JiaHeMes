using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ChargePadLine.Common.Config;
using ChargePadLine.DbContexts;

namespace ChargePadLine.WebApi
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDBServices(this IServiceCollection services, IConfiguration configuration)
        {
            var builder = WebApplication.CreateBuilder();
            // 添加日志服务
            services.AddLogging(logging =>
            {
                logging.AddLog4Net()
                       .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
                       .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None)
                       .AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning);
            });

            #region SWagger配置
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ChargePadLine.Api",
                    Version = "v1",
                    Description = "ChargePadLine.Api",
                });

                // 解决重复路由/冲突时抛出的异常（取首个描述），防止 swagger.json 500
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                //添加安全定义
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT授权",
                    Name = "Authorization", //默认的参数名
                    In = ParameterLocation.Header,//放于请求头中
                    Type = SecuritySchemeType.ApiKey,//类型
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                //添加安全要求
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
            #endregion

            //添加数据库迁移配置
            services.AddOptions<MigrationConfig>().Bind(configuration.GetSection("Migration"));

            //添加Quartz配置
            services.AddOptions<QuartzConfig>().Bind(configuration.GetSection("Quartz"));
            //使用pgSql
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("AppDbContext"));
                options.UseNpgsql(s => s.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
                //抛出sql文本
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            //使用报表数据库
            services.AddDbContext<ReportDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("ReportDbContext"));
                options.UseNpgsql(s => s.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
                //抛出sql文本
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });
            return services;
        }
    }
}
