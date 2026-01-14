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
        private bool _masterOk;
        private bool _masterNg;
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
            get => _masterNg;
            private set
            {
                if (_masterNg != value)
                {
                    _masterNg = value;
                    OnPropertyChanged(nameof(干区气密MasterNg));
                }
            }
        }


        public bool 干区气密MasterOk
        {
            get => _masterOk;
            private set
            {
                if (_masterOk != value)
                {
                    _masterOk = value;
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
        public void UpdateData(bool req, bool resp, string sn, bool masterok, bool masterng)
        {
            干区气密MasterReq = req;
            干区气密MasterResp = resp;
            干区气密MasterSn = sn;
            干区气密MasterOk = masterok;
            干区气密MasterNg = masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
