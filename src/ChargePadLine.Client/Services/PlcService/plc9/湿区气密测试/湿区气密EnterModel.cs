using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc9.湿区气密测试
{
    public class 湿区气密EnterModel : INotifyPropertyChanged
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
        public bool 湿区气密EnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(湿区气密EnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 湿区气密EnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(湿区气密EnterResp));
                }
            }
        }

        public bool 湿区气密EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(湿区气密EnterNg));
                }
            }
        }


        public bool 湿区气密EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(湿区气密EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 湿区气密EnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(湿区气密EnterSn));
                }
            }
        }

        public string 湿区气密Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(湿区气密Status));
                }
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng,string status)
        {
            湿区气密EnterReq = req;
            湿区气密EnterResp = resp;
            湿区气密EnterSn = sn;
            湿区气密EnterOk = enterok;
            湿区气密EnterNg = enterng;
            湿区气密Status = status;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
