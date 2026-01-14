using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc2
{
    /// <summary>
    /// 电机腔气密测试数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class 电机腔气密MasterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _masterOk;
        private bool _masterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 气密MasterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(气密MasterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 气密MasterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(气密MasterResp));
                }
            }
        }

        public bool 气密MasterNg
        {
            get => _masterNg;
            private set
            {
                if (_masterNg != value)
                {
                    _masterNg = value;
                    OnPropertyChanged(nameof(气密MasterNg));
                }
            }
        }


        public bool 气密MasterOk
        {
            get => _masterOk;
            private set
            {
                if (_masterOk != value)
                {
                    _masterOk = value;
                    OnPropertyChanged(nameof(气密MasterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 气密MasterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(气密MasterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool masterok, bool masterng)
        {
            气密MasterReq = req;
            气密MasterResp = resp;
            气密MasterSn = sn;
            气密MasterOk = masterok;
            气密MasterNg = masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}