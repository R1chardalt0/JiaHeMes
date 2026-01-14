using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc4.后盖超声波焊接
{
    public class 后盖超声波焊接MasterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _masterOk;
        private bool _masterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 后盖超声波焊接MasterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(后盖超声波焊接MasterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 后盖超声波焊接MasterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(后盖超声波焊接MasterResp));
                }
            }
        }

        public bool 后盖超声波焊接MasterNg
        {
            get => _masterNg;
            private set
            {
                if (_masterNg != value)
                {
                    _masterNg = value;
                    OnPropertyChanged(nameof(后盖超声波焊接MasterNg));
                }
            }
        }


        public bool 后盖超声波焊接MasterOk
        {
            get => _masterOk;
            private set
            {
                if (_masterOk != value)
                {
                    _masterOk = value;
                    OnPropertyChanged(nameof(后盖超声波焊接MasterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 后盖超声波焊接MasterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(后盖超声波焊接MasterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool masterok, bool masterng)
        {
            后盖超声波焊接MasterReq = req;
            后盖超声波焊接MasterResp = resp;
            后盖超声波焊接MasterSn = sn;
            后盖超声波焊接MasterOk = masterok;
            后盖超声波焊接MasterNg = masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
