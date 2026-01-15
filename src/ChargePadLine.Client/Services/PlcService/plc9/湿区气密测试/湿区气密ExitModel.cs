using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc9.湿区气密测试
{
    public class 湿区气密ExitModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _exitOk;
        private bool _exitNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 湿区气密ExitReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(湿区气密ExitReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 湿区气密ExitResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(湿区气密ExitResp));
                }
            }
        }

        public bool 湿区气密ExitNg
        {
            get => _exitNg;
            private set
            {
                if (_exitNg != value)
                {
                    _exitNg = value;
                    OnPropertyChanged(nameof(湿区气密ExitNg));
                }
            }
        }


        public bool 湿区气密ExitOk
        {
            get => _exitOk;
            private set
            {
                if (_exitOk != value)
                {
                    _exitOk = value;
                    OnPropertyChanged(nameof(湿区气密ExitOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 湿区气密ExitSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(湿区气密ExitSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool Exitok, bool Exitng)
        {
            湿区气密ExitReq = req;
            湿区气密ExitResp = resp;
            湿区气密ExitSn = sn;
            湿区气密ExitOk = Exitok;
            湿区气密ExitNg = Exitng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
