using ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;

namespace ChargePadLine.Client.ViewModels
{
    /// <summary>
    /// 性能检查And热铆视图模型
    /// </summary>
    public class 性能检查And热铆ViewModel:ViewModelBase
    {
        private readonly PCBA性能检测EnterModel _PCBA性能检测EnterModel;
        private readonly PCBA性能检测ExitModel _PCBA性能检测ExitModel;

        private readonly 热铆EnterModel _热铆EnterModel;
        private readonly 热铆ExitModel _热铆ExitModel;

        public 性能检查And热铆ViewModel(PCBA性能检测EnterModel PCBA性能检测EnterModel, PCBA性能检测ExitModel PCBA性能检测ExitModel, 热铆EnterModel 热铆EnterModel, 热铆ExitModel 热铆ExitModel)
        {
            _PCBA性能检测EnterModel = PCBA性能检测EnterModel;
            _PCBA性能检测ExitModel = PCBA性能检测ExitModel;
            _热铆EnterModel = 热铆EnterModel;
            _热铆ExitModel = 热铆ExitModel;


            PCBA性能检测EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            PCBA性能检测ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            热铆EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            热铆ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }


        public string 性能检查Status => _PCBA性能检测EnterModel.性能检查Status;
        /// <summary>
        /// PCBA性能检测 - 请求状态
        /// </summary>
        public bool 性能检查EnterReq => _PCBA性能检测EnterModel.性能检查EnterReq;

        /// <summary>
        /// PCBA性能检测 - 响应状态
        /// </summary>
        public bool 性能检查EnterResp => _PCBA性能检测EnterModel.性能检查EnterResp;

        /// <summary>
        /// PCBA性能检测 - 序列号
        /// </summary>
        public string 性能检查EnterSn => _PCBA性能检测EnterModel.性能检查EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 性能检查EnterOk => _PCBA性能检测EnterModel.性能检查EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 性能检查EnterNg => _PCBA性能检测EnterModel.性能检查EnterNg;


        /// <summary>
        /// PCBA性能检测 - 请求状态
        /// </summary>
        public bool 性能检测ExitReq => _PCBA性能检测ExitModel.性能检测ExitReq;

        /// <summary>
        /// PCBA性能检测 - 响应状态
        /// </summary>
        public bool 性能检测ExitResp => _PCBA性能检测ExitModel.性能检测ExitResp;

        /// <summary>
        /// PCBA性能检测 - 序列号
        /// </summary>
        public string 性能检测ExitSn => _PCBA性能检测ExitModel.性能检测ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 性能检测ExitOk => _PCBA性能检测ExitModel.性能检测ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 性能检测ExitNg => _PCBA性能检测ExitModel.性能检测ExitNg;


        public string 热铆Status => _热铆EnterModel.热铆Status;
        /// <summary>
        /// 热铆 - 请求状态
        /// </summary>
        public bool 热铆EnterReq => _热铆EnterModel.热铆EnterReq;

        /// <summary>
        /// 热铆 - 响应状态
        /// </summary>
        public bool 热铆EnterResp => _热铆EnterModel.热铆EnterResp;

        /// <summary>
        /// 热铆 - 序列号
        /// </summary>
        public string 热铆EnterSn => _热铆EnterModel.热铆EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 热铆EnterOk => _热铆EnterModel.热铆EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 热铆EnterNg => _热铆EnterModel.热铆EnterNg;



        /// <summary>
        /// 热铆 - 请求状态
        /// </summary>
        public bool 热铆ExitReq => _热铆ExitModel.热铆ExitReq;

        /// <summary>
        /// 热铆 - 响应状态
        /// </summary>
        public bool 热铆ExitResp => _热铆ExitModel.热铆ExitResp;

        /// <summary>
        /// 热铆 - 序列号
        /// </summary>
        public string 热铆ExitSn => _热铆ExitModel.热铆ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 热铆ExitOk => _热铆ExitModel.热铆ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 热铆ExitNg => _热铆ExitModel.热铆ExitNg;
    }
}
