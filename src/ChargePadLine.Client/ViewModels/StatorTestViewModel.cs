using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;

namespace ChargePadLine.Client.ViewModels
{
    /// <summary>
    /// 定子测试视图模型
    /// </summary>
    public class StatorTestViewModel : ViewModelBase
    {
        private readonly StatorEnterModel _statorTestDataEnterService;
        private readonly StatorExitModel _statorTestDataExitService;
        private readonly RingEnterModel _ringEnter;
        private readonly RingExitModel _ringExit;


       public StatorTestViewModel(StatorEnterModel statorTestDataEnterService,StatorExitModel statorTestDataExitService, RingEnterModel ringEnter, RingExitModel ringExit)
        {
            _statorTestDataEnterService = statorTestDataEnterService;
            _statorTestDataExitService= statorTestDataExitService;

            _ringEnter = ringEnter;
            _ringExit = ringExit;
            statorTestDataEnterService.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _statorTestDataExitService.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _ringEnter.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _ringExit.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool Req => _statorTestDataEnterService.Req;

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool Resp => _statorTestDataEnterService.Resp;

        /// <summary>
        /// 序列号
        /// </summary>
        public string Sn => _statorTestDataEnterService.Sn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool EnterOk => _statorTestDataEnterService.EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool EnterNg => _statorTestDataEnterService.EnterNg;


        /// <summary>
        /// 请求状态
        /// </summary>
        public bool ExitReq => _statorTestDataExitService.ExitReq;

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool ExitResp => _statorTestDataExitService.ExitResp;

        /// <summary>
        /// 序列号
        /// </summary>
        public string ExitSn => _statorTestDataExitService.Sn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool ExitOk => _statorTestDataExitService.ExitOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool ExitNg => _statorTestDataExitService.ExitNg;


        /// <summary>
        /// 请求状态
        /// </summary>
        public bool RingEnterReq => _ringEnter.RingEnterReq;

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool RingEnterResp => _ringEnter.RingEnterResp;

        /// <summary>
        /// 序列号
        /// </summary>
        public string RingEnterSn => _ringEnter.RingEnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool RingEnterOk => _ringEnter.RingEnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool RingEnterNg => _ringEnter.RingEnterNg;



        /// <summary>
        /// 请求状态
        /// </summary>
        public bool RingExitReq => _ringExit.RingExitReq;

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool RingExitResp => _ringExit.RingExitResp;

        /// <summary>
        /// 序列号
        /// </summary>
        public string RingExitSn => _ringExit.RingExitSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool RingExitOk => _ringExit.RingExitOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool RingExitNg => _ringExit.RingExitNg;
    }
}