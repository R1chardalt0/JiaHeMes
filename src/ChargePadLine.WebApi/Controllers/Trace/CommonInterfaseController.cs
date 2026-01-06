using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargePadLine.Service.Trace.Impl;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  public class CommonInterfaseController : BaseController
  {
    private readonly ICommonInterfaseService _iCommonInterfaseService;

    public CommonInterfaseController(ICommonInterfaseService iCommonInterfaseService)
    {
            _iCommonInterfaseService = iCommonInterfaseService;
    }

        
    }
}
