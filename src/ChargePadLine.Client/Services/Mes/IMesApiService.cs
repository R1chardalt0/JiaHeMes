using ChargePadLine.Client.Services.Mes.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.Mes
{
    public interface IMesApiService
    {
        /// <summary>
        /// 数据校验接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<RespDto> UploadCheck(ReqDto req);

        /// <summary>
        /// 数据收集接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<RespDto> UploadData(ReqDto req);

        /// <summary>
        /// 点检上传接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<RespDto> UploadMaster(ReqDto req);
    }
}
