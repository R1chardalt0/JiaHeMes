using ChargePadLine.Client.Services.PlcService.Plc2;
using ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷;
using ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试;

namespace ChargePadLine.Client.ViewModels
{
    /// <summary>
    /// 导热胶和气密视图模型
    /// </summary>
    public class 导热胶And气密ViewModel : ViewModelBase
    {
        private readonly 导热胶涂敷EnterModel _导热胶涂敷EnterModel;
        private readonly 导热胶涂敷ExitModel _导热胶涂敷ExitModel;
        private readonly 电机腔气密测试EnterModel _电机腔气密测试EnterModel;
        private readonly 电机腔气密测试ExitModel _电机腔气密测试ExitModel;

        public 导热胶And气密ViewModel(导热胶涂敷EnterModel 导热胶涂敷EnterModel, 导热胶涂敷ExitModel 导热胶涂敷ExitModel, 电机腔气密测试EnterModel 电机腔气密测试EnterModel, 电机腔气密测试ExitModel 电机腔气密测试ExitModel)
        {
            _导热胶涂敷EnterModel = 导热胶涂敷EnterModel;
            _导热胶涂敷ExitModel = 导热胶涂敷ExitModel;
            _电机腔气密测试EnterModel = 电机腔气密测试EnterModel;
            _电机腔气密测试ExitModel = 电机腔气密测试ExitModel;

            导热胶涂敷EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            导热胶涂敷ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            电机腔气密测试EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            电机腔气密测试ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };

        }

        /// <summary>
        /// 导热胶 - 请求状态
        /// </summary>
        public bool 导热胶EnterReq => _导热胶涂敷EnterModel.导热胶EnterReq;

        /// <summary>
        /// 导热胶 - 响应状态
        /// </summary>
        public bool 导热胶EnterResp => _导热胶涂敷EnterModel.导热胶EnterResp;

        /// <summary>
        /// 导热胶 - 序列号
        /// </summary>
        public string 导热胶EnterSn => _导热胶涂敷EnterModel.导热胶EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 导热胶EnterOk => _导热胶涂敷EnterModel.导热胶EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 导热胶EnterNg => _导热胶涂敷EnterModel.导热胶EnterNg;



        /// <summary>
        /// 导热胶 - 请求状态
        /// </summary>
        public bool 导热胶ExitReq => _导热胶涂敷ExitModel.导热胶ExitReq;

        /// <summary>
        /// 导热胶 - 响应状态
        /// </summary>
        public bool 导热胶ExitResp => _导热胶涂敷ExitModel.导热胶ExitResp;

        /// <summary>
        /// 导热胶 - 序列号
        /// </summary>
        public string 导热胶ExitSn => _导热胶涂敷ExitModel.导热胶ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 导热胶ExitOk => _导热胶涂敷ExitModel.导热胶ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 导热胶ExitNg => _导热胶涂敷ExitModel.导热胶ExitNg;


        /// <summary>
        /// 气密 - 请求状态
        /// </summary>
        public bool 气密EnterReq => _电机腔气密测试EnterModel.气密EnterReq;

        /// <summary>
        /// 气密 - 响应状态
        /// </summary>
        public bool 气密EnterResp => _电机腔气密测试EnterModel.气密EnterResp;

        /// <summary>
        /// 气密 - 序列号
        /// </summary>
        public string 气密EnterSn => _电机腔气密测试EnterModel.气密EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 气密EnterOk => _电机腔气密测试EnterModel.气密EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 气密EnterNg => _电机腔气密测试EnterModel.气密EnterNg;


        /// <summary>
        /// 气密 - 请求状态
        /// </summary>
        public bool 气密ExitReq => _电机腔气密测试ExitModel.气密ExitReq;

        /// <summary>
        /// 气密 - 响应状态
        /// </summary>
        public bool 气密ExitResp => _电机腔气密测试ExitModel.气密ExitResp;

        /// <summary>
        /// 气密 - 序列号
        /// </summary>
        public string 气密ExitSn => _电机腔气密测试ExitModel.气密ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 气密ExitOk => _电机腔气密测试ExitModel.气密ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 气密ExitNg => _电机腔气密测试ExitModel.气密ExitNg;
    }
}
