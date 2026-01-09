using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc2
{
    /// <summary>
    /// 电机腔气密测试数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class 电机腔气密测试ExitModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 气密ExitReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(气密ExitReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 气密ExitResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(气密ExitResp));
                }
            }
        }

        public bool 气密ExitNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(气密ExitNg));
                }
            }
        }


        public bool 气密ExitOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(气密ExitOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 气密ExitSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(气密ExitSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng)
        {
            气密ExitReq = req;
            气密ExitResp = resp;
            气密ExitSn = sn;
            气密ExitOk = enterok;
            气密ExitNg = enterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}