using ChargePadLine.Client.Services.PlcService.plc8.旋融焊;

namespace ChargePadLine.Client.ViewModels
{
    public class 旋融焊ViewModel:ViewModelBase
    {
        private readonly 旋融焊EnterModel _旋融焊EnterModel;
        private readonly 旋融焊ExitModel _旋融焊ExitModel;

        public 旋融焊ViewModel(旋融焊EnterModel 旋融焊EnterModel, 旋融焊ExitModel 旋融焊ExitModel)
        {
            _旋融焊EnterModel = 旋融焊EnterModel;
            _旋融焊ExitModel = 旋融焊ExitModel;

            旋融焊EnterModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
            旋融焊ExitModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }


        public string 旋融焊Status => _旋融焊EnterModel.旋融焊Status;
        /// <summary>
        /// 旋融焊 - 请求状态
        /// </summary>
        public bool 旋融焊EnterReq => _旋融焊EnterModel.旋融焊EnterReq;

        /// <summary>
        /// 旋融焊 - 响应状态
        /// </summary>
        public bool 旋融焊EnterResp => _旋融焊EnterModel.旋融焊EnterResp;

        /// <summary>
        /// 旋融焊 - 序列号
        /// </summary>
        public string 旋融焊EnterSn => _旋融焊EnterModel.旋融焊EnterSn;

        /// <summary>
        /// 进站ok状态
        /// </summary>
        public bool 旋融焊EnterOk => _旋融焊EnterModel.旋融焊EnterOk;

        /// <summary>
        /// 进站ng状态
        /// </summary>
        public bool 旋融焊EnterNg => _旋融焊EnterModel.旋融焊EnterNg;


        /// <summary>
        /// 旋融焊 - 请求状态
        /// </summary>
        public bool 旋融焊ExitReq => _旋融焊ExitModel.旋融焊ExitReq;

        /// <summary>
        /// 旋融焊 - 响应状态
        /// </summary>
        public bool 旋融焊ExitResp => _旋融焊ExitModel.旋融焊ExitResp;

        /// <summary>
        /// 旋融焊 - 序列号
        /// </summary>
        public string 旋融焊ExitSn => _旋融焊ExitModel.旋融焊ExitSn;

        /// <summary>
        /// 出站ok状态
        /// </summary>
        public bool 旋融焊ExitOk => _旋融焊ExitModel.旋融焊ExitOk;

        /// <summary>
        /// 出站ng状态
        /// </summary>
        public bool 旋融焊ExitNg => _旋融焊ExitModel.旋融焊ExitNg;
    }
}
