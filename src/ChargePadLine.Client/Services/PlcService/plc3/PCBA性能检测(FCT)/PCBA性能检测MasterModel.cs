using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_
{
    public class PCBA性能检测MasterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _masterOk;
        private bool _masterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 性能检测MasterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(性能检测MasterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 性能检测MasterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(性能检测MasterResp));
                }
            }
        }

        public bool 性能检测MasterNg
        {
            get => _masterNg;
            private set
            {
                if (_masterNg != value)
                {
                    _masterNg = value;
                    OnPropertyChanged(nameof(性能检测MasterNg));
                }
            }
        }


        public bool 性能检测MasterOk
        {
            get => _masterOk;
            private set
            {
                if (_masterOk != value)
                {
                    _masterOk = value;
                    OnPropertyChanged(nameof(性能检测MasterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 性能检测MasterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(性能检测MasterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool masterok, bool masterng)
        {
            性能检测MasterReq = req;
            性能检测MasterResp = resp;
            性能检测MasterSn = sn;
            性能检测MasterOk = masterok;
            性能检测MasterNg = masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
