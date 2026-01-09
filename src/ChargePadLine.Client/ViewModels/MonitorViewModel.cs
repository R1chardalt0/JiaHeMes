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

        public MonitorViewModel(StatorTestViewModel statorTestViewModel, 导热胶And气密ViewModel 导热胶And气密ViewModel)
        {
            StatorTestViewModel = statorTestViewModel;
            this.导热胶And气密ViewModel = 导热胶And气密ViewModel;
        }
    }
}
