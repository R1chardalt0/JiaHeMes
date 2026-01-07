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

        public MonitorViewModel(StatorTestViewModel statorTestViewModel)
        {
            StatorTestViewModel = statorTestViewModel;
        }
    }
}
