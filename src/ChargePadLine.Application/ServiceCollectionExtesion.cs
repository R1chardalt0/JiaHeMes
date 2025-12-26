using ChargePadLine.Application.Trace.Production.BatchQueue;
using ChargePadLine.Application.Trace.Production.Recipes;
using ChargePadLine.Application.Trace.Production.TraceInformation;
using ChargePadLine.Application.Trace.Production.WorkOrders;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Service.Migration;
using ChargePadLine.Service.Migration.Impl;
using ChargePadLine.Service.OperationLog;
using ChargePadLine.Service.OperationLog.Impl;
using ChargePadLine.Service.Systems;
using ChargePadLine.Service.Systems.Impl;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Application
{
    public static class ServiceCollectionExtesion
    {
        public static IServiceCollection AddAppCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 物料批队列服务
            services.AddScoped<IMaterialBatchQueueItemRepo, MaterialBatchQueueItemRepo>();
            services.AddScoped<MaterialBatchQueueBiz>();
            services.AddScoped<WorkOrderBiz>();
            // BOM配方服务
            services.AddScoped<IBomRecipeRepository, BomRecipeRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<BomRecipeBiz>();
            
            // VSN控制服务
            services.AddScoped<ICtrlVsnsService, CtrlVsnsService>();

            return services;
        }
    }
}
