using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配
{
    public class RingEnterModel : INotifyPropertyChanged
    {
        private bool _ringEnterReq;
        private bool _ringEnterResp;
        private bool _ringEnterOk;
        private bool _ringEnterNg;
        private string _ringEnterSn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool RingEnterReq
        {
            get => _ringEnterReq;
            private set
            {
                if (_ringEnterReq != value)
                {
                    _ringEnterReq = value;
                    OnPropertyChanged(nameof(RingEnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool RingEnterResp
        {
            get => _ringEnterResp;
            private set
            {
                if (_ringEnterResp != value)
                {
                    _ringEnterResp = value;
                    OnPropertyChanged(nameof(RingEnterResp));
                }
            }
        }

        public bool RingEnterNg
        {
            get => _ringEnterNg;
            private set
            {
                if (_ringEnterNg != value)
                {
                    _ringEnterNg = value;
                    OnPropertyChanged(nameof(RingEnterNg));
                }
            }
        }


        public bool RingEnterOk
        {
            get => _ringEnterOk;
            private set
            {
                if (_ringEnterOk != value)
                {
                    _ringEnterOk = value;
                    OnPropertyChanged(nameof(RingEnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string RingEnterSn
        {
            get => _ringEnterSn;
            private set
            {
                if (_ringEnterSn != value)
                {
                    _ringEnterSn = value;
                    OnPropertyChanged(nameof(RingEnterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng)
        {
            RingEnterReq = req;
            RingEnterResp = resp;
            RingEnterSn = sn;
            RingEnterOk = enterok;
            RingEnterNg = enterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
