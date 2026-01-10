using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Client.ViewModels;

namespace ChargePadLine.Client.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {
        public StatorTestViewModel StatorTestViewModel { get; }
        public 导热胶And气密ViewModel 导热胶And气密ViewModel { get; }
        public 性能检查And热铆ViewModel 性能检查And热铆ViewModel { get; }
        public 后盖超声波焊接And干区气密测试ViewModel 后盖超声波焊接And干区气密测试ViewModel { get; }
        public 转子充磁And装配ViewModel 转子充磁And装配ViewModel { get; }
        public 止推垫片装配ViewModel 止推垫片装配ViewModel { get; }

        public MonitorViewModel(StatorTestViewModel statorTestViewModel, 导热胶And气密ViewModel 导热胶And气密ViewModel, 性能检查And热铆ViewModel 性能检查And热铆ViewModel, 后盖超声波焊接And干区气密测试ViewModel 后盖超声波焊接And干区气密测试ViewModel, 转子充磁And装配ViewModel 转子充磁And装配ViewModel, 止推垫片装配ViewModel 止推垫片装配ViewModel)
        {
            this.StatorTestViewModel = statorTestViewModel;
            this.导热胶And气密ViewModel = 导热胶And气密ViewModel;
            this.性能检查And热铆ViewModel = 性能检查And热铆ViewModel;
            this.后盖超声波焊接And干区气密测试ViewModel = 后盖超声波焊接And干区气密测试ViewModel;
            this.转子充磁And装配ViewModel = 转子充磁And装配ViewModel;
            this.止推垫片装配ViewModel = 止推垫片装配ViewModel;
        }
    }
}
