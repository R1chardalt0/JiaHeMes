using ChargePadLine.Client.Services.PlcService.plc5.转子充磁与装配;

namespace ChargePadLine.Client.ViewModels
{
    public class 转子充磁And装配ViewModel : ViewModelBase
    {
        private readonly 转子充磁与装配EnterModel _转子充磁与装配EnterModel;
        private readonly 转子充磁与装配ExitModel _转子充磁与装配ExitModel;
        private readonly 转子充磁与装配MasterModel _转子充磁与装配MasterModel;

        public 转子充磁And装配ViewModel(转子充磁与装配EnterModel 转子充磁与装配EnterModel, 转子充磁与装配ExitModel 转子充磁与装配ExitModel, 转子充磁与装配MasterModel 转子充磁与装配MasterModel)
        {
            _转子充磁与装配EnterModel = 转子充磁与装配EnterModel;
            _转子充磁与装配ExitModel = 转子充磁与装配ExitModel;
            _转子充磁与装配MasterModel = 转子充磁与装配MasterModel;

            _转子充磁与装配EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _转子充磁与装配ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _转子充磁与装配MasterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };

        }

        public string 转子充磁与装配Status => _转子充磁与装配EnterModel.转子充磁与装配Status;
        /// <summary>
        /// 转子充磁与装配 - 请求状态
        /// </summary>
        public bool 转子充磁与装配EnterReq => _转子充磁与装配EnterModel.转子充磁与装配EnterReq;

        /// <summary>
        /// 转子充磁与装配 - 响应状态
        /// </summary>
        public bool 转子充磁与装配EnterResp => _转子充磁与装配EnterModel.转子充磁与装配EnterResp;

        /// <summary>
        /// 转子充磁与装配 - 序列号
        /// </summary>
        public string 转子充磁与装配EnterSn => _转子充磁与装配EnterModel.转子充磁与装配EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 转子充磁与装配EnterOk => _转子充磁与装配EnterModel.转子充磁与装配EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 转子充磁与装配EnterNg => _转子充磁与装配EnterModel.转子充磁与装配EnterNg;


        /// <summary>
        /// 转子充磁与装配 - 请求状态
        /// </summary>
        public bool 转子充磁与装配ExitReq => _转子充磁与装配ExitModel.转子充磁与装配ExitReq;

        /// <summary>
        /// 转子充磁与装配 - 响应状态
        /// </summary>
        public bool 转子充磁与装配ExitResp => _转子充磁与装配ExitModel.转子充磁与装配ExitResp;

        /// <summary>
        /// 转子充磁与装配 - 序列号
        /// </summary>
        public string 转子充磁与装配ExitSn => _转子充磁与装配ExitModel.转子充磁与装配ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 转子充磁与装配ExitOk => _转子充磁与装配ExitModel.转子充磁与装配ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 转子充磁与装配ExitNg => _转子充磁与装配ExitModel.转子充磁与装配ExitNg;

        /// <summary>
        /// 转子充磁与装配 - 请求状态
        /// </summary>
        public bool 转子充磁与装配MasterReq => _转子充磁与装配MasterModel.转子充磁与装配MasterReq;

        /// <summary>
        /// 转子充磁与装配 - 响应状态
        /// </summary>
        public bool 转子充磁与装配MasterResp => _转子充磁与装配MasterModel.转子充磁与装配MasterResp;

        /// <summary>
        /// 转子充磁与装配 - 序列号
        /// </summary>
        public string 转子充磁与装配MasterSn => _转子充磁与装配MasterModel.转子充磁与装配MasterSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 转子充磁与装配MasterOk => _转子充磁与装配MasterModel.转子充磁与装配MasterOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 转子充磁与装配MasterNg => _转子充磁与装配MasterModel.转子充磁与装配MasterNg;
    }
}
