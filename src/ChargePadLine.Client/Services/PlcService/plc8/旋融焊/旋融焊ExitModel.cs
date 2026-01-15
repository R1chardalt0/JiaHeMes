using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc8.旋融焊
{
    public class 旋融焊ExitModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _exitOk;
        private bool _exitNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 旋融焊ExitReq
        {
            get => _req;
            private set
            {
                _req = value;
                OnPropertyChanged(nameof(旋融焊ExitReq));
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 旋融焊ExitResp
        {
            get => _resp;
            private set
            {
                _resp = value;
                OnPropertyChanged(nameof(旋融焊ExitResp));
            }
        }

        public bool 旋融焊ExitNg
        {
            get => _exitNg;
            private set
            {
                _exitNg = value;
                OnPropertyChanged(nameof(旋融焊ExitNg));
            }
        }


        public bool 旋融焊ExitOk
        {
            get => _exitOk;
            private set
            {
                _exitOk = value;
                OnPropertyChanged(nameof(旋融焊ExitOk));
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 旋融焊ExitSn
        {
            get => _sn;
            private set
            {
                _sn = value;
                OnPropertyChanged(nameof(旋融焊ExitSn));
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool Exitok, bool Exitng)
        {
            旋融焊ExitReq = req;
            旋融焊ExitResp = resp;
            旋融焊ExitSn = sn;
            旋融焊ExitOk = Exitok;
            旋融焊ExitNg = Exitng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
