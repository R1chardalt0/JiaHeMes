using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Common.Config
{
    public class QuartzConfig
    {
        /// <summary>
        /// 调度器配置
        /// </summary>
        public SchedulerConfig Scheduler { get; set; } = new SchedulerConfig();
        
        /// <summary>
        /// 线程池配置
        /// </summary>
        public ThreadPoolConfig ThreadPool { get; set; } = new ThreadPoolConfig();
        
        /// <summary>
        /// 作业存储配置
        /// </summary>
        public JobStoreConfig JobStore { get; set; } = new JobStoreConfig();
        
        /// <summary>
        /// 插件配置
        /// </summary>
        public PluginConfig Plugin { get; set; } = new PluginConfig();
        
        /// <summary>
        /// 作业配置
        /// </summary>
        public JobsConfig Jobs { get; set; } = new JobsConfig();
    }
    
    /// <summary>
    /// 调度器配置
    /// </summary>
    public class SchedulerConfig
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        public string InstanceName { get; set; } = "ChargePadLineScheduler";
        
        /// <summary>
        /// 实例ID
        /// </summary>
        public string InstanceId { get; set; } = "AUTO";
    }
    
    /// <summary>
    /// 线程池配置
    /// </summary>
    public class ThreadPoolConfig
    {
        /// <summary>
        /// 线程池类型
        /// </summary>
        public string Type { get; set; } = "Quartz.Simpl.SimpleThreadPool, Quartz";
        
        /// <summary>
        /// 线程优先级
        /// </summary>
        public string ThreadPriority { get; set; } = "Normal";
        
        /// <summary>
        /// 线程数量
        /// </summary>
        public int ThreadCount { get; set; } = 10;
    }
    
    /// <summary>
    /// 作业存储配置
    /// </summary>
    public class JobStoreConfig
    {
        /// <summary>
        /// 存储类型
        /// </summary>
        public string Type { get; set; } = "Quartz.Simpl.RAMJobStore, Quartz";
        
        /// <summary>
        /// 失火阈值（毫秒）
        /// </summary>
        public int MisfireThreshold { get; set; } = 60000;
    }
    
    /// <summary>
    /// 插件配置
    /// </summary>
    public class PluginConfig
    {
        /// <summary>
        /// 作业历史插件配置
        /// </summary>
        public JobHistoryPluginConfig JobHistory { get; set; } = new JobHistoryPluginConfig();
        
        /// <summary>
        /// 关闭钩子插件配置
        /// </summary>
        public ShutdownHookPluginConfig ShutdownHook { get; set; } = new ShutdownHookPluginConfig();
    }
    
    /// <summary>
    /// 作业历史插件配置
    /// </summary>
    public class JobHistoryPluginConfig
    {
        /// <summary>
        /// 插件类型
        /// </summary>
        public string Type { get; set; } = "Quartz.Plugin.History.LoggingJobHistoryPlugin, Quartz.Plugins";
        
        /// <summary>
        /// 日志级别
        /// </summary>
        public string LogLevel { get; set; } = "Info";
    }
    
    /// <summary>
    /// 关闭钩子插件配置
    /// </summary>
    public class ShutdownHookPluginConfig
    {
        /// <summary>
        /// 插件类型
        /// </summary>
        public string Type { get; set; } = "Quartz.Plugin.Management.ShutdownHookPlugin, Quartz.Plugins";
    }
    
    /// <summary>
    /// 作业配置
    /// </summary>
    public class JobsConfig
    {
        /// <summary>
        /// 数据迁移作业配置
        /// </summary>
        public MigrationJobConfig Migration { get; set; } = new MigrationJobConfig();
    }
    
    /// <summary>
    /// 数据迁移作业配置
    /// </summary>
    public class MigrationJobConfig
    {
        /// <summary>
        /// Cron表达式
        /// </summary>
        public string CronExpression { get; set; } = "0 0 2 * * ?";
        
        /// <summary>
        /// 时区ID
        /// </summary>
        public string TimeZoneId { get; set; } = "China Standard Time";
    }
}
