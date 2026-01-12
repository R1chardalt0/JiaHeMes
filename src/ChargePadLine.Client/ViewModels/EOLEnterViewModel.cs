using ChargePadLine.Client.Services.PlcService.plc10.EOL测试;

namespace ChargePadLine.Client.ViewModels
{
    public class EOLEnterViewModel:ViewModelBase
    {
        private readonly EOLEnterModel _EOLEnterModel;
        private readonly EOLExitModel _EOLExitModel;

        public EOLEnterViewModel(EOLEnterModel eOLEnterModel, EOLExitModel eOLExitModel)
        {
            _EOLEnterModel = eOLEnterModel;
            _EOLExitModel = eOLExitModel;

            eOLEnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            eOLExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }

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
    }
}
