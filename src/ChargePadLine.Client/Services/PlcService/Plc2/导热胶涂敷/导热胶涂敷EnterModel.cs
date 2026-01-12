using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷
{
    /// <summary>
    /// 导热胶涂敷数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class 导热胶涂敷EnterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 导热胶EnterReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(导热胶EnterReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 导热胶EnterResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(导热胶EnterResp));
                }
            }
        }

        public bool 导热胶EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(导热胶EnterNg));
                }
            }
        }


        public bool 导热胶EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(导热胶EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 导热胶EnterSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(导热胶EnterSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng)
        {
            导热胶EnterReq = req;
            导热胶EnterResp = resp;
            导热胶EnterSn = sn;
            导热胶EnterOk = enterok;
            导热胶EnterNg = enterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}