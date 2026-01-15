using ChargePadLine.Client.Services.PlcService.plc11.安装支架;
using ChargePadLine.Client.Services.PlcService.plc11.客户码激光刻印;

namespace ChargePadLine.Client.ViewModels
{
    public class 安装支架And激光刻印ViewModel : ViewModelBase
    {
        private readonly 安装支架EnterModel _安装支架EnterModel;
        private readonly 安装支架ExitModel _安装支架ExitModel;
        private readonly 安装支架MasterModel _安装支架MasterModel;
        private readonly 激光刻印EnterModel _激光刻印EnterModel;
        private readonly 激光刻印ExitModel _激光刻印ExitModel;
        private readonly 激光刻印MasterModel _激光刻印MasterModel;

        public 安装支架And激光刻印ViewModel(安装支架EnterModel 安装支架EnterModel, 安装支架ExitModel 安装支架ExitModel, 激光刻印EnterModel 激光刻印EnterModel = null, 激光刻印ExitModel 激光刻印ExitModel = null, 安装支架MasterModel 安装支架MasterModel = null, 激光刻印MasterModel 激光刻印MasterModel = null)
        {
            _安装支架EnterModel = 安装支架EnterModel;
            _安装支架ExitModel = 安装支架ExitModel;
            _安装支架MasterModel = 安装支架MasterModel;
            _激光刻印EnterModel = 激光刻印EnterModel;
            _激光刻印ExitModel = 激光刻印ExitModel;
            _激光刻印MasterModel = 激光刻印MasterModel;

            _安装支架EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _安装支架ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _安装支架MasterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _激光刻印EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _激光刻印ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _激光刻印MasterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };

        }
        public string 安装支架Status => _安装支架EnterModel.安装支架Status;
        /// <summary>
        /// 安装支架 - 请求状态
        /// </summary>
        public bool 安装支架EnterReq => _安装支架EnterModel.安装支架EnterReq;

        /// <summary>
        /// 安装支架 - 响应状态
        /// </summary>
        public bool 安装支架EnterResp => _安装支架EnterModel.安装支架EnterResp;

        /// <summary>
        /// 安装支架 - 序列号
        /// </summary>
        public string 安装支架EnterSn => _安装支架EnterModel.安装支架EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 安装支架EnterOk => _安装支架EnterModel.安装支架EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 安装支架EnterNg => _安装支架EnterModel.安装支架EnterNg;


        /// <summary>
        /// 安装支架 - 请求状态
        /// </summary>
        public bool 安装支架ExitReq => _安装支架ExitModel.安装支架ExitReq;

        /// <summary>
        /// 安装支架 - 响应状态
        /// </summary>
        public bool 安装支架ExitResp => _安装支架ExitModel.安装支架ExitResp;

        /// <summary>
        /// 安装支架 - 序列号
        /// </summary>
        public string 安装支架ExitSn => _安装支架ExitModel.安装支架ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 安装支架ExitOk => _安装支架ExitModel.安装支架ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 安装支架ExitNg => _安装支架ExitModel.安装支架ExitNg;

        /// <summary>
        /// 安装支架 - 请求状态
        /// </summary>
        public bool 安装支架MasterReq => _安装支架MasterModel.安装支架MasterReq;

        /// <summary>
        /// 安装支架 - 响应状态
        /// </summary>
        public bool 安装支架MasterResp => _安装支架MasterModel.安装支架MasterResp;

        /// <summary>
        /// 安装支架 - 序列号
        /// </summary>
        public string 安装支架MasterSn => _安装支架MasterModel.安装支架MasterSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 安装支架MasterOk => _安装支架MasterModel.安装支架MasterOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 安装支架MasterNg => _安装支架MasterModel.安装支架MasterNg;


        public string 激光刻印Status => _激光刻印EnterModel.激光刻印Status;
        /// <summary>
        /// 激光刻印 - 请求状态
        /// </summary>
        public bool 激光刻印EnterReq => _激光刻印EnterModel.激光刻印EnterReq;

        /// <summary>
        /// 激光刻印 - 响应状态
        /// </summary>
        public bool 激光刻印EnterResp => _激光刻印EnterModel.激光刻印EnterResp;

        /// <summary>
        /// 激光刻印 - 序列号
        /// </summary>
        public string 激光刻印EnterSn => _激光刻印EnterModel.激光刻印EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 激光刻印EnterOk => _激光刻印EnterModel.激光刻印EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 激光刻印EnterNg => _激光刻印EnterModel.激光刻印EnterNg;



        /// <summary>
        /// 激光刻印 - 请求状态
        /// </summary>
        public bool 激光刻印ExitReq => _激光刻印ExitModel.激光刻印ExitReq;

        /// <summary>
        /// 激光刻印 - 响应状态
        /// </summary>
        public bool 激光刻印ExitResp => _激光刻印ExitModel.激光刻印ExitResp;

        /// <summary>
        /// 激光刻印 - 序列号
        /// </summary>
        public string 激光刻印ExitSn => _激光刻印ExitModel.激光刻印ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 激光刻印ExitOk => _激光刻印ExitModel.激光刻印ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 激光刻印ExitNg => _激光刻印ExitModel.激光刻印ExitNg;

        /// <summary>
        /// 激光刻印 - 请求状态
        /// </summary>
        public bool 激光刻印MasterReq => _激光刻印MasterModel.激光刻印MasterReq;

        /// <summary>
        /// 激光刻印 - 响应状态
        /// </summary>
        public bool 激光刻印MasterResp => _激光刻印MasterModel.激光刻印MasterResp;

        /// <summary>
        /// 激光刻印 - 序列号
        /// </summary>
        public string 激光刻印MasterSn => _激光刻印MasterModel.激光刻印MasterSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 激光刻印MasterOk => _激光刻印MasterModel.激光刻印MasterOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 激光刻印MasterNg => _激光刻印MasterModel.激光刻印MasterNg;
    }
}