using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc2
{
    /// <summary>
    /// 导热胶涂敷数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class 导热胶涂敷ExitModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _exitOk;
        private bool _exitNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 导热胶ExitReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(导热胶ExitReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 导热胶ExitResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(导热胶ExitResp));
                }
            }
        }

        public bool 导热胶ExitNg
        {
            get => _exitNg;
            private set
            {
                if (_exitNg != value)
                {
                    _exitNg = value;
                    OnPropertyChanged(nameof(导热胶ExitNg));
                }
            }
        }


        public bool 导热胶ExitOk
        {
            get => _exitOk;
            private set
            {
                if (_exitOk != value)
                {
                    _exitOk = value;
                    OnPropertyChanged(nameof(导热胶ExitOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 导热胶ExitSn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(导热胶ExitSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng)
        {
            导热胶ExitReq = req;
            导热胶ExitResp = resp;
            导热胶ExitSn = sn;
            导热胶ExitOk = enterok;
            导热胶ExitNg = enterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}