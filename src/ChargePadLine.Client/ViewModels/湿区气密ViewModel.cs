using ChargePadLine.Client.Services.PlcService.plc9.湿区气密测试;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.ViewModels
{
    public class 湿区气密ViewModel:ViewModelBase
    {
        private readonly 湿区气密EnterModel _湿区气密EnterModel;
        private readonly 湿区气密ExitModel _湿区气密ExitModel;

        public 湿区气密ViewModel(湿区气密EnterModel 湿区气密EnterModel, 湿区气密ExitModel 湿区气密ExitModel)
        {
            _湿区气密EnterModel = 湿区气密EnterModel;
            _湿区气密ExitModel = 湿区气密ExitModel;

            湿区气密EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            湿区气密ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }

        /// <summary>
        /// 湿区气密 - 请求状态
        /// </summary>
        public bool 湿区气密EnterReq => _湿区气密EnterModel.湿区气密EnterReq;

        /// <summary>
        /// 湿区气密 - 响应状态
        /// </summary>
        public bool 湿区气密EnterResp => _湿区气密EnterModel.湿区气密EnterResp;

        /// <summary>
        /// 湿区气密 - 序列号
        /// </summary>
        public string 湿区气密EnterSn => _湿区气密EnterModel.湿区气密EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 湿区气密EnterOk => _湿区气密EnterModel.湿区气密EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 湿区气密EnterNg => _湿区气密EnterModel.湿区气密EnterNg;


        /// <summary>
        /// 湿区气密 - 请求状态
        /// </summary>
        public bool 湿区气密ExitReq => _湿区气密ExitModel.湿区气密ExitReq;

        /// <summary>
        /// 湿区气密 - 响应状态
        /// </summary>
        public bool 湿区气密ExitResp => _湿区气密ExitModel.湿区气密ExitResp;

        /// <summary>
        /// 湿区气密 - 序列号
        /// </summary>
        public string 湿区气密ExitSn => _湿区气密ExitModel.湿区气密ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 湿区气密ExitOk => _湿区气密ExitModel.湿区气密ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 湿区气密ExitNg => _湿区气密ExitModel.湿区气密ExitNg;
    }
}
