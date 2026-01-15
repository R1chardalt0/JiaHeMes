using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc3.热铆
{
    public class 热铆MasterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _masterOk;
        private bool _masterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 热铆MasterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(热铆MasterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 热铆MasterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(热铆MasterResp));
                }
            }
        }

        public bool 热铆MasterNg
        {
            get => _masterNg;
            private set
            {
                if (_masterNg != value)
                {
                    _masterNg = value;
                    OnPropertyChanged(nameof(热铆MasterNg));
                }
            }
        }


        public bool 热铆MasterOk
        {
            get => _masterOk;
            private set
            {
                if (_masterOk != value)
                {
                    _masterOk = value;
                    OnPropertyChanged(nameof(热铆MasterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 热铆MasterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(热铆MasterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool masterok, bool masterng)
        {
            热铆MasterReq = req;
            热铆MasterResp = resp;
            热铆MasterSn = sn;
            热铆MasterOk = masterok;
            热铆MasterNg = masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
