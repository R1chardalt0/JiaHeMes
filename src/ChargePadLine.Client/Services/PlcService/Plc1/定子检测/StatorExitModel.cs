using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc1.定子检测
{
    /// <summary>
    /// 定子检测数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class StatorExitModel  : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _exitOk;
        private bool _exitNg;
        private string _exitSn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool ExitReq
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(ExitReq));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool ExitResp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(ExitResp));
                }
            }
        }

        public bool ExitNg
        {
            get => _exitNg;
            private set
            {
                if (_exitNg != value)
                {
                    _exitNg = value;
                    OnPropertyChanged(nameof(ExitNg));
                }
            }
        }


        public bool ExitOk
        {
            get => _exitOk;
            private set
            {
                if (_exitOk != value)
                {
                    _exitOk = value;
                    OnPropertyChanged(nameof(ExitOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string ExitSn
        {
            get => _exitSn;
            private set
            {
                if (_exitSn != value)
                {
                    _exitSn = value;
                    OnPropertyChanged(nameof(ExitSn));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn,bool exitOk, bool exitNg)
        {
            ExitReq = req;
            ExitResp = resp;
            ExitSn = sn;
            ExitOk = exitOk;
            ExitNg = exitNg;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}