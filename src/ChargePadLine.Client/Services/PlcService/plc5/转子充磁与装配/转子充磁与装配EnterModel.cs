using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc5.转子充磁与装配
{
    public class 转子充磁与装配EnterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 转子充磁与装配EnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(转子充磁与装配EnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 转子充磁与装配EnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(转子充磁与装配EnterResp));
                }
            }
        }

        public bool 转子充磁与装配EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(转子充磁与装配EnterNg));
                }
            }
        }


        public bool 转子充磁与装配EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(转子充磁与装配EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 转子充磁与装配EnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(转子充磁与装配EnterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng)
        {
            转子充磁与装配EnterReq = req;
            转子充磁与装配EnterResp = resp;
            转子充磁与装配EnterSn = sn;
            转子充磁与装配EnterOk = enterok;
            转子充磁与装配EnterNg = enterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
