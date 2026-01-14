using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试
{
    /// <summary>
    /// 电机腔气密测试数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class 电机腔气密EnterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;
        private string _status = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 气密EnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(气密EnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 气密EnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(气密EnterResp));
                }
            }
        }

        public bool 气密EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(气密EnterNg));
                }
            }
        }


        public bool 气密EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(气密EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 气密EnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(气密EnterSn));
                }
            }
        }

        public string 气密Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(气密Status));
                }
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng,string statusMessage)
        {
            气密EnterReq = req;
            气密EnterResp = resp;
            气密EnterSn = sn;
            气密EnterOk = enterok;
            气密EnterNg = enterng;
            气密Status = statusMessage;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}