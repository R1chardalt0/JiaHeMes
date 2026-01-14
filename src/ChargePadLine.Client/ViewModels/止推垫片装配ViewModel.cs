using ChargePadLine.Client.Services.PlcService.plc7.止推垫片装配;

namespace ChargePadLine.Client.ViewModels
{
    public class 止推垫片装配ViewModel : ViewModelBase
    {
        private readonly 止推垫片装配EnterModel _止推垫片装配EnterModel;
        private readonly 止推垫片装配ExitModel _止推垫片装配ExitModel;
        private readonly 止推垫片装配MasterModel _止推垫片装配MasterModel;

        public 止推垫片装配ViewModel(止推垫片装配EnterModel 止推垫片装配EnterModel, 止推垫片装配ExitModel 止推垫片装配ExitModel, 止推垫片装配MasterModel 止推垫片装配MasterModel)
        {
            _止推垫片装配EnterModel = 止推垫片装配EnterModel;
            _止推垫片装配ExitModel = 止推垫片装配ExitModel;
            _止推垫片装配MasterModel = 止推垫片装配MasterModel;

            _止推垫片装配EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _止推垫片装配ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _止推垫片装配MasterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }

        public string 止推垫片装配Status => _止推垫片装配EnterModel.止推垫片装配Status;
        /// <summary>
        /// 止推垫片装配 - 请求状态
        /// </summary>
        public bool 止推垫片装配EnterReq => _止推垫片装配EnterModel.止推垫片装配EnterReq;

        /// <summary>
        /// 止推垫片装配 - 响应状态
        /// </summary>
        public bool 止推垫片装配EnterResp => _止推垫片装配EnterModel.止推垫片装配EnterResp;

        /// <summary>
        /// 止推垫片装配 - 序列号
        /// </summary>
        public string 止推垫片装配EnterSn => _止推垫片装配EnterModel.止推垫片装配EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 止推垫片装配EnterOk => _止推垫片装配EnterModel.止推垫片装配EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 止推垫片装配EnterNg => _止推垫片装配EnterModel.止推垫片装配EnterNg;


        /// <summary>
        /// 止推垫片装配 - 请求状态
        /// </summary>
        public bool 止推垫片装配ExitReq => _止推垫片装配ExitModel.止推垫片装配ExitReq;

        /// <summary>
        /// 止推垫片装配 - 响应状态
        /// </summary>
        public bool 止推垫片装配ExitResp => _止推垫片装配ExitModel.止推垫片装配ExitResp;

        /// <summary>
        /// 止推垫片装配 - 序列号
        /// </summary>
        public string 止推垫片装配ExitSn => _止推垫片装配ExitModel.止推垫片装配ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 止推垫片装配ExitOk => _止推垫片装配ExitModel.止推垫片装配ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 止推垫片装配ExitNg => _止推垫片装配ExitModel.止推垫片装配ExitNg;


        /// <summary>
        /// 止推垫片装配 - 请求状态
        /// </summary>
        public bool 止推垫片装配MasterReq => _止推垫片装配MasterModel.止推垫片装配MasterReq;

        /// <summary>
        /// 止推垫片装配 - 响应状态
        /// </summary>
        public bool 止推垫片装配MasterResp => _止推垫片装配MasterModel.止推垫片装配MasterResp;

        /// <summary>
        /// 止推垫片装配 - 序列号
        /// </summary>
        public string 止推垫片装配MasterSn => _止推垫片装配MasterModel.止推垫片装配MasterSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 止推垫片装配MasterOk => _止推垫片装配MasterModel.止推垫片装配MasterOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 止推垫片装配MasterNg => _止推垫片装配MasterModel.止推垫片装配MasterNg;
    }
}
