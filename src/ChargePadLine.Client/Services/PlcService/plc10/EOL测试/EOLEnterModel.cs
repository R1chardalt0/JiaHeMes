using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc10.EOL测试
{
    public class EOLEnterModel : INotifyPropertyChanged
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
        public bool EolEnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(EolEnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool EolEnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(EolEnterResp));
                }
            }
        }

        public bool EolEnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(EolEnterNg));
                }
            }
        }


        public bool EolEnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(EolEnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string EolEnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(EolEnterSn));
                }
            }
        }

        public string EolStatus
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(EolStatus));
                }
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng,string status)
        {
            EolEnterReq = req;
            EolEnterResp = resp;
            EolEnterSn = sn;
            EolEnterOk = enterok;
            EolEnterNg = enterng;
            EolStatus = status;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
