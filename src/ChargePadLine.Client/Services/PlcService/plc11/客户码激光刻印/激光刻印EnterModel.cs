using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc11.客户码激光刻印
{
    public class 激光刻印EnterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;
        private string _激光刻印status = string.Empty;
        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 激光刻印EnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(激光刻印EnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 激光刻印EnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(激光刻印EnterResp));
                }
            }
        }

        public bool 激光刻印EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(激光刻印EnterNg));
                }
            }
        }


        public bool 激光刻印EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(激光刻印EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 激光刻印EnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(激光刻印EnterSn));
                }
            }
        }

        public string 激光刻印Status
        {
            get => _激光刻印status;
            private set
            {
                if (_激光刻印status != value)
                {
                    _激光刻印status = value;
                    OnPropertyChanged(nameof(激光刻印Status));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng, string status)
        {
            激光刻印EnterReq = req;
            激光刻印EnterResp = resp;
            激光刻印EnterSn = sn;
            激光刻印EnterOk = enterok;
            激光刻印EnterNg = enterng;
            激光刻印Status = status;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
