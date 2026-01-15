using ChargePadLine.Client.Services.PlcService.plc10.EOL测试;

namespace ChargePadLine.Client.ViewModels
{
    public class EOLEnterViewModel : ViewModelBase
    {
        private readonly EOLEnterModel _EOLEnterModel;
        private readonly EOLExitModel _EOLExitModel;
        private readonly EOLMasterModel _EOLMasterModel;

        public EOLEnterViewModel(EOLEnterModel EOLEnterModel, EOLExitModel EOLExitModel, EOLMasterModel EOLMasterModel)
        {
            _EOLEnterModel = EOLEnterModel;
            _EOLExitModel = EOLExitModel;
            _EOLMasterModel = EOLMasterModel;

            _EOLEnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _EOLExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _EOLMasterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }


        public string EolStatus => _EOLEnterModel.EolStatus;
        /// <summary>
        /// EOLE - 请求状态
        /// </summary>
        public bool EolEnterReq => _EOLEnterModel.EolEnterReq;

        /// <summary>
        /// EOLE - 响应状态
        /// </summary>
        public bool EolEnterResp => _EOLEnterModel.EolEnterResp;

        /// <summary>
        /// EOLE - 序列号
        /// </summary>
        public string EolEnterSn => _EOLEnterModel.EolEnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool EolEnterOk => _EOLEnterModel.EolEnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool EolEnterNg => _EOLEnterModel.EolEnterNg;


        /// <summary>
        /// EOLE - 请求状态
        /// </summary>
        public bool EolExitReq => _EOLExitModel.EolExitReq;

        /// <summary>
        /// EOLE - 响应状态
        /// </summary>
        public bool EolExitResp => _EOLExitModel.EolExitResp;

        /// <summary>
        /// EOLE - 序列号
        /// </summary>
        public string EolExitSn => _EOLExitModel.EolExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool EolExitOk => _EOLExitModel.EolExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool EolExitNg => _EOLExitModel.EolExitNg;

        /// <summary>
        /// EOLE - 请求状态
        /// </summary>
        public bool EolMasterReq => _EOLMasterModel.EolMasterReq;

        /// <summary>
        /// EOLE - 响应状态
        /// </summary>
        public bool EolMasterResp => _EOLMasterModel.EolMasterResp;

        /// <summary>
        /// EOLE - 序列号
        /// </summary>
        public string EolMasterSn => _EOLMasterModel.EolMasterSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool EolMasterOk => _EOLMasterModel.EolMasterOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool EolMasterNg => _EOLMasterModel.EolMasterNg;
    }
}
