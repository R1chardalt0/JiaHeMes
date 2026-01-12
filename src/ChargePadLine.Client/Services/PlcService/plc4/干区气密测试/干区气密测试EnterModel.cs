using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc4.干区气密测试
{
    public class 干区气密测试EnterModel:INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 干区气密测试EnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(干区气密测试EnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 干区气密测试EnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(干区气密测试EnterResp));
                }
            }
        }

        public bool 干区气密测试EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(干区气密测试EnterNg));
                }
            }
        }


        public bool 干区气密测试EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(干区气密测试EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 干区气密测试EnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(干区气密测试EnterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng)
        {
            干区气密测试EnterReq = req;
            干区气密测试EnterResp = resp;
            干区气密测试EnterSn = sn;
            干区气密测试EnterOk = enterok;
            干区气密测试EnterNg = enterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}