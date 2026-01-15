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
        private readonly StatorMasterModel _statorMasterModel;

        private readonly RingEnterModel _ringEnter;
        private readonly RingExitModel _ringExit;
        private readonly RingMasterModel _ringMaster;


        public StatorTestViewModel(StatorEnterModel statorTestDataEnterService, StatorExitModel statorTestDataExitService, RingEnterModel ringEnter, RingExitModel ringExit, StatorMasterModel statorMasterModel, RingMasterModel ringMaster)
        {
            _statorTestDataEnterService = statorTestDataEnterService;
            _statorTestDataExitService = statorTestDataExitService;
            _statorMasterModel = statorMasterModel;

            _ringEnter = ringEnter;
            _ringExit = ringExit;
            _ringMaster = ringMaster;
            _statorTestDataEnterService.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _statorTestDataExitService.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _statorMasterModel.PropertyChanged += (sender, e) =>
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
            _ringMaster.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }

        public string 定子检测Status => _statorTestDataEnterService.定子检测Status;

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
        public string ExitSn => _statorTestDataExitService.ExitSn;

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
        public bool MasterReq => _statorMasterModel.MasterReq;

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool MasterResp => _statorMasterModel.MasterResp;

        /// <summary>
        /// 序列号
        /// </summary>
        public string MasterSn => _statorMasterModel.MasterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool MasterOk => _statorMasterModel.MasterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool MasterNg => _statorMasterModel.MasterNg;



        public string O型圈装配Status => _ringEnter.O型圈装配Status;
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




        /// <summary>
        /// 请求状态
        /// </summary>
        public bool RingMasterReq => _ringMaster.RingMasterReq;

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool RingMasterResp => _ringMaster.RingMasterResp;

        /// <summary>
        /// 序列号
        /// </summary>
        public string RingMasterSn => _ringMaster.RingMasterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool RingMasterOk => _ringMaster.RingMasterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool RingMasterNg => _ringMaster.RingMasterNg;
    }
}