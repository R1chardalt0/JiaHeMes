using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc7.止推垫片装配
{
    public class 止推垫片装配ExitModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _exitOk;
        private bool _exitNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 止推垫片装配ExitReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(止推垫片装配ExitReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 止推垫片装配ExitResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(止推垫片装配ExitResp));
                }
            }
        }

        public bool 止推垫片装配ExitNg
        {
            get => _exitNg;
            private set
            {
                if (_exitNg != value)
                {
                    _exitNg = value;
                    OnPropertyChanged(nameof(止推垫片装配ExitNg));
                }
            }
        }


        public bool 止推垫片装配ExitOk
        {
            get => _exitOk;
            private set
            {
                if (_exitOk != value)
                {
                    _exitOk = value;
                    OnPropertyChanged(nameof(止推垫片装配ExitOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 止推垫片装配ExitSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(止推垫片装配ExitSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool Exitok, bool Exitng)
        {
            止推垫片装配ExitReq = req;
            止推垫片装配ExitResp = resp;
            止推垫片装配ExitSn = sn;
            止推垫片装配ExitOk = Exitok;
            止推垫片装配ExitNg = Exitng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
