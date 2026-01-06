using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.Product;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 工单服务实现
  /// </summary>
  public class CommonInterfaseService : ICommonInterfaseService
    {

    private readonly AppDbContext _dbContext;

    public CommonInterfaseService(AppDbContext dbContext)
    {
      _dbContext = dbContext;
    }

        public Task<List<ProductList>> GetDeviceInfoList()
        {
            throw new NotImplementedException();
        }
    }
}