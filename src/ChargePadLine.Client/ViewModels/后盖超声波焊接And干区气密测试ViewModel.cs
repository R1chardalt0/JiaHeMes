using ChargePadLine.Client.Services.PlcService.plc4.后盖超声波焊接;
using ChargePadLine.Client.Services.PlcService.plc4.干区气密测试;

namespace ChargePadLine.Client.ViewModels
{
    public class 后盖超声波焊接And干区气密测试ViewModel : ViewModelBase
    {
        private readonly 后盖超声波焊接EnterModel _后盖超声波焊接EnterModel;
        private readonly 后盖超声波焊接ExitModel _后盖超声波焊接ExitModel;
        private readonly 后盖超声波焊接MasterModel _后盖超声波焊接MasterModel;

        private readonly 干区气密EnterModel _干区气密EnterModel;
        private readonly 干区气密ExitModel _干区气密ExitModel;
        private readonly 干区气密MasterModel _干区气密MasterModel;

        public 后盖超声波焊接And干区气密测试ViewModel(后盖超声波焊接EnterModel 后盖超声波焊接EnterModel, 后盖超声波焊接ExitModel 后盖超声波焊接ExitModel, 干区气密EnterModel 干区气密EnterModel, 干区气密ExitModel 干区气密ExitModel, 后盖超声波焊接MasterModel 后盖超声波焊接MasterModel, 干区气密MasterModel 干区气密MasterModel)
        {
            _后盖超声波焊接EnterModel = 后盖超声波焊接EnterModel;
            _后盖超声波焊接ExitModel = 后盖超声波焊接ExitModel;
            _后盖超声波焊接MasterModel = 后盖超声波焊接MasterModel;
            _干区气密EnterModel = 干区气密EnterModel;
            _干区气密ExitModel = 干区气密ExitModel;
            _干区气密MasterModel = 干区气密MasterModel;

            _后盖超声波焊接EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _后盖超声波焊接ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _后盖超声波焊接MasterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };

            _干区气密EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _干区气密ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            _干区气密MasterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };


        }

        public string 后盖超声波焊接Status => _后盖超声波焊接EnterModel.后盖超声波焊接Status;
        /// <summary>
        /// 后盖超声波焊接 - 请求状态
        /// </summary>
        public bool 后盖超声波焊接EnterReq => _后盖超声波焊接EnterModel.后盖超声波焊接EnterReq;

        /// <summary>
        /// 后盖超声波焊接 - 响应状态
        /// </summary>
        public bool 后盖超声波焊接EnterResp => _后盖超声波焊接EnterModel.后盖超声波焊接EnterResp;

        /// <summary>
        /// 后盖超声波焊接 - 序列号
        /// </summary>
        public string 后盖超声波焊接EnterSn => _后盖超声波焊接EnterModel.后盖超声波焊接EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 后盖超声波焊接EnterOk => _后盖超声波焊接EnterModel.后盖超声波焊接EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 后盖超声波焊接EnterNg => _后盖超声波焊接EnterModel.后盖超声波焊接EnterNg;


        /// <summary>
        /// 后盖超声波焊接 - 请求状态
        /// </summary>
        public bool 后盖超声波焊接ExitReq => _后盖超声波焊接ExitModel.后盖超声波焊接ExitReq;

        /// <summary>
        /// 后盖超声波焊接 - 响应状态
        /// </summary>
        public bool 后盖超声波焊接ExitResp => _后盖超声波焊接ExitModel.后盖超声波焊接ExitResp;

        /// <summary>
        /// 后盖超声波焊接 - 序列号
        /// </summary>
        public string 后盖超声波焊接ExitSn => _后盖超声波焊接ExitModel.后盖超声波焊接ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 后盖超声波焊接ExitOk => _后盖超声波焊接ExitModel.后盖超声波焊接ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 后盖超声波焊接ExitNg => _后盖超声波焊接ExitModel.后盖超声波焊接ExitNg;

        /// <summary>
        /// 后盖超声波焊接 - 请求状态
        /// </summary>
        public bool 后盖超声波焊接MasterReq => _后盖超声波焊接MasterModel.后盖超声波焊接MasterReq;

        /// <summary>
        /// 后盖超声波焊接 - 响应状态
        /// </summary>
        public bool 后盖超声波焊接MasterResp => _后盖超声波焊接MasterModel.后盖超声波焊接MasterResp;

        /// <summary>
        /// 后盖超声波焊接 - 序列号
        /// </summary>
        public string 后盖超声波焊接MasterSn => _后盖超声波焊接MasterModel.后盖超声波焊接MasterSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 后盖超声波焊接MasterOk => _后盖超声波焊接MasterModel.后盖超声波焊接MasterOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 后盖超声波焊接MasterNg => _后盖超声波焊接MasterModel.后盖超声波焊接MasterNg;



        public string 干区气密测试Status => _干区气密EnterModel.干区气密测试Status;
        /// <summary>
        /// 干区气密测试 - 请求状态
        /// </summary>
        public bool 干区气密测试EnterReq => _干区气密EnterModel.干区气密测试EnterReq;

        /// <summary>
        /// 干区气密测试 - 响应状态
        /// </summary>
        public bool 干区气密测试EnterResp => _干区气密EnterModel.干区气密测试EnterResp;

        /// <summary>
        /// 干区气密测试 - 序列号
        /// </summary>
        public string 干区气密测试EnterSn => _干区气密EnterModel.干区气密测试EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 干区气密测试EnterOk => _干区气密EnterModel.干区气密测试EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 干区气密测试EnterNg => _干区气密EnterModel.干区气密测试EnterNg;



        /// <summary>
        /// 干区气密测试 - 请求状态
        /// </summary>
        public bool 干区气密测试ExitReq => _干区气密ExitModel.干区气密测试ExitReq;

        /// <summary>
        /// 干区气密测试 - 响应状态
        /// </summary>
        public bool 干区气密测试ExitResp => _干区气密ExitModel.干区气密测试ExitResp;

        /// <summary>
        /// 干区气密测试 - 序列号
        /// </summary>
        public string 干区气密测试ExitSn => _干区气密ExitModel.干区气密测试ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 干区气密测试ExitOk => _干区气密ExitModel.干区气密测试ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 干区气密测试ExitNg => _干区气密ExitModel.干区气密测试ExitNg;

        /// <summary>
        /// 干区气密测试 - 请求状态
        /// </summary>
        public bool 干区气密MasterReq => _干区气密MasterModel.干区气密MasterReq;

        /// <summary>
        /// 干区气密测试 - 响应状态
        /// </summary>
        public bool 干区气密MasterResp => _干区气密MasterModel.干区气密MasterResp;

        /// <summary>
        /// 干区气密测试 - 序列号
        /// </summary>
        public string 干区气密MasterSn => _干区气密MasterModel.干区气密MasterSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 干区气密MasterOk => _干区气密MasterModel.干区气密MasterOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 干区气密MasterNg => _干区气密MasterModel.干区气密MasterNg;
    }
}
