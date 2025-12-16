using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;

namespace ChargePadLine.Service.Migration
{
    /// <summary>
    /// Quartz配置扩展类
    /// </summary>
    public static class QuartzConfigurationExtensions
    {
        /// <summary>
        /// 添加数据迁移定时作业配置
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="cronExpression">Cron表达式，默认为每天凌晨2点执行</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddMigrationQuartzJobs(this IServiceCollection services, string cronExpression = "0 0 2 * * ?")
        {
            // 添加Quartz服务
            services.AddQuartz(q =>
            {
                // 使用依赖注入方式创建作业
                q.UseMicrosoftDependencyInjectionJobFactory();
                
                // 注册数据迁移作业
                var jobKey = new JobKey("MigrationJob", "Migration");
                q.AddJob<Impl.MigrationJob>(options =>
                {
                    options.WithIdentity(jobKey);
                    options.StoreDurably();
                    options.WithDescription("数据迁移定时作业");
                });
                
                // 注册触发器，使用cron表达式配置执行时间
                q.AddTrigger(trigger =>
                trigger.ForJob(jobKey)
                    .WithIdentity("MigrationJobTrigger", "Migration")
                    .WithDescription("数据迁移作业触发器")
                    .WithCronSchedule(cronExpression, x =>
                    {
                        x.WithMisfireHandlingInstructionDoNothing();
                    })
                );
            });
            
            // 添加Quartz托管服务
            services.AddQuartzHostedService(q =>
            {
                q.WaitForJobsToComplete = true;
                q.AwaitApplicationStarted = true;
            });
            
            return services;
        }
    }
}