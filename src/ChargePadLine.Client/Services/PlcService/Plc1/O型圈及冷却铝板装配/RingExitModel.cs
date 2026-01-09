using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配
{
    public class RingExitModel : INotifyPropertyChanged
    {
        private bool _ringExitReq;
        private bool _ringExitResp;
        private bool _ringExitOk;
        private bool _ringExitNg;
        private string _ringExitSn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool RingExitReq
        {
            get => _ringExitReq;
            private set
            {
                if (_ringExitReq != value)
                {
                    _ringExitReq = value;
                    OnPropertyChanged(nameof(RingExitReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool RingExitResp
        {
            get => _ringExitResp;
            private set
            {
                if (_ringExitResp != value)
                {
                    _ringExitResp = value;
                    OnPropertyChanged(nameof(RingExitResp));
                }
            }
        }

        public bool RingExitNg
        {
            get => _ringExitNg;
            private set
            {
                if (_ringExitNg != value)
                {
                    _ringExitNg = value;
                    OnPropertyChanged(nameof(RingExitNg));
                }
            }
        }


        public bool RingExitOk
        {
            get => _ringExitOk;
            private set
            {
                if (_ringExitOk != value)
                {
                    _ringExitOk = value;
                    OnPropertyChanged(nameof(RingExitOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string RingExitSn
        {
            get => _ringExitSn;
            private set
            {
                if (_ringExitSn != value)
                {
                    _ringExitSn = value;
                    OnPropertyChanged(nameof(RingExitSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool Exitok, bool Exitng)
        {
            RingExitReq = req;
            RingExitResp = resp;
            RingExitSn = sn;
            RingExitOk = Exitok;
            RingExitNg = Exitng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
