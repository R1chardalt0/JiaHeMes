using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc4.干区气密测试
{
    public class 干区气密MasterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _MasterOk;
        private bool _MasterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 干区气密MasterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(干区气密MasterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 干区气密MasterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(干区气密MasterResp));
                }
            }
        }

        public bool 干区气密MasterNg
        {
            get => _MasterNg;
            private set
            {
                if (_MasterNg != value)
                {
                    _MasterNg = value;
                    OnPropertyChanged(nameof(干区气密MasterNg));
                }
            }
        }


        public bool 干区气密MasterOk
        {
            get => _MasterOk;
            private set
            {
                if (_MasterOk != value)
                {
                    _MasterOk = value;
                    OnPropertyChanged(nameof(干区气密MasterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 干区气密MasterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(干区气密MasterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool Masterok, bool Masterng)
        {
            干区气密MasterReq = req;
            干区气密MasterResp = resp;
            干区气密MasterSn = sn;
            干区气密MasterOk = Masterok;
            干区气密MasterNg = Masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
