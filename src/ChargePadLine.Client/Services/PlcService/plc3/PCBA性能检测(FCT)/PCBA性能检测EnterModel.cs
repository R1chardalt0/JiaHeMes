using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_
{
    public class PCBA性能检测EnterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;
        private string _status = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 性能检查EnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(性能检查EnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 性能检查EnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(性能检查EnterResp));
                }
            }
        }

        public bool 性能检查EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(性能检查EnterNg));
                }
            }
        }


        public bool 性能检查EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(性能检查EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 性能检查EnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(性能检查EnterSn));
                }
            }
        }

        public string 性能检查Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(性能检查Status));
                }
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng,string status)
        {
            性能检查EnterReq = req;
            性能检查EnterResp = resp;
            性能检查EnterSn = sn;
            性能检查EnterOk = enterok;
            性能检查EnterNg = enterng;
            性能检查Status = status;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
