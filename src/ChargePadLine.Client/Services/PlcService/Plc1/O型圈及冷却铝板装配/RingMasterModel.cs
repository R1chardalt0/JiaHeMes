using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配
{
    public class RingMasterModel : INotifyPropertyChanged
    {
        private bool _ringMasterReq;
        private bool _ringMasterResp;
        private bool _ringMasterOk;
        private bool _ringMasterNg;
        private string _ringMasterSn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool RingMasterReq
        {
            get => _ringMasterReq;
            private set
            {
                if (_ringMasterReq != value)
                {
                    _ringMasterReq = value;
                    OnPropertyChanged(nameof(RingMasterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool RingMasterResp
        {
            get => _ringMasterResp;
            private set
            {
                if (_ringMasterResp != value)
                {
                    _ringMasterResp = value;
                    OnPropertyChanged(nameof(RingMasterResp));
                }
            }
        }

        public bool RingMasterNg
        {
            get => _ringMasterNg;
            private set
            {
                if (_ringMasterNg != value)
                {
                    _ringMasterNg = value;
                    OnPropertyChanged(nameof(RingMasterNg));
                }
            }
        }


        public bool RingMasterOk
        {
            get => _ringMasterOk;
            private set
            {
                if (_ringMasterOk != value)
                {
                    _ringMasterOk = value;
                    OnPropertyChanged(nameof(RingMasterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string RingMasterSn
        {
            get => _ringMasterSn;
            private set
            {
                if (_ringMasterSn != value)
                {
                    _ringMasterSn = value;
                    OnPropertyChanged(nameof(RingMasterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool masterok, bool masterng)
        {
            RingMasterReq = req;
            RingMasterResp = resp;
            RingMasterSn = sn;
            RingMasterOk = masterok;
            RingMasterNg =masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
