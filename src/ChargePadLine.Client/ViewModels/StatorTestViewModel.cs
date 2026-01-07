using ChargePadLine.Client.Services.PlcService;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.ViewModels;

namespace ChargePadLine.Client.ViewModels
{
    /// <summary>
    /// 定子测试视图模型
    /// </summary>
    public class StatorTestViewModel : ViewModelBase
    {
        private readonly StatorTestDataService _statorTestDataService;

        public StatorTestViewModel(StatorTestDataService statorTestDataService)
        {
            _statorTestDataService = statorTestDataService;
            _statorTestDataService.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };
        }

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool Req => _statorTestDataService.Req;

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool Resp => _statorTestDataService.Resp;

        /// <summary>
        /// 序列号
        /// </summary>
        public string Sn => _statorTestDataService.Sn;
    }
}