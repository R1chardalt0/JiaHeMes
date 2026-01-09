using ChargePadLine.Client.Services.PlcService.Plc2;

namespace ChargePadLine.Client.ViewModels
{
    /// <summary>
    /// 导热胶和气密视图模型
    /// </summary>
    public class 导热胶And气密ViewModel : ViewModelBase
    {
        private readonly 导热胶涂敷DataService _导热胶涂敷DataService;
        private readonly 电机腔气密测试DataService _电机腔气密测试DataService;

        public 导热胶And气密ViewModel(
            导热胶涂敷DataService 导热胶涂敷DataService,
            电机腔气密测试DataService 电机腔气密测试DataService)
        {
            _导热胶涂敷DataService = 导热胶涂敷DataService;
            _电机腔气密测试DataService = 电机腔气密测试DataService;

            _导热胶涂敷DataService.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Req":
                        OnPropertyChanged(nameof(导热胶Req));
                        break;
                    case "Resp":
                        OnPropertyChanged(nameof(导热胶Resp));
                        break;
                    case "Sn":
                        OnPropertyChanged(nameof(导热胶Sn));
                        break;
                }
            };

            _电机腔气密测试DataService.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Req":
                        OnPropertyChanged(nameof(气密Req));
                        break;
                    case "Resp":
                        OnPropertyChanged(nameof(气密Resp));
                        break;
                    case "Sn":
                        OnPropertyChanged(nameof(气密Sn));
                        break;
                }
            };
        }

        /// <summary>
        /// 导热胶 - 请求状态
        /// </summary>
        public bool 导热胶Req => _导热胶涂敷DataService.Req;

        /// <summary>
        /// 导热胶 - 响应状态
        /// </summary>
        public bool 导热胶Resp => _导热胶涂敷DataService.Resp;

        /// <summary>
        /// 导热胶 - 序列号
        /// </summary>
        public string 导热胶Sn => _导热胶涂敷DataService.Sn;

        /// <summary>
        /// 气密 - 请求状态
        /// </summary>
        public bool 气密Req => _电机腔气密测试DataService.Req;

        /// <summary>
        /// 气密 - 响应状态
        /// </summary>
        public bool 气密Resp => _电机腔气密测试DataService.Resp;

        /// <summary>
        /// 气密 - 序列号
        /// </summary>
        public string 气密Sn => _电机腔气密测试DataService.Sn;
    }
}
