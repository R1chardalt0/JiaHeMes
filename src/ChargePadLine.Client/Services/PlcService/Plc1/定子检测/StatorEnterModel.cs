using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc1.定子检测
{
    /// <summary>
    /// 定子检测数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class StatorEnterModel : INotifyPropertyChanged
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
        public bool Req
        {
            get => _req;
            private set
            {
                if (_req != value)
                {
                    _req = value;
                    OnPropertyChanged(nameof(Req));
                }
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool Resp
        {
            get => _resp;
            private set
            {
                if (_resp != value)
                {
                    _resp = value;
                    OnPropertyChanged(nameof(Resp));
                }
            }
        }

        public bool EnterNg
        {
            get => _enterNg;
            private set
            {
                if (_enterNg != value)
                {
                    _enterNg = value;
                    OnPropertyChanged(nameof(EnterNg));
                }
            }
        }


        public bool EnterOk
        {
            get => _enterOk;
            private set
            {
                if (_enterOk != value)
                {
                    _enterOk = value;
                    OnPropertyChanged(nameof(EnterOk));
                }
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string Sn
        {
            get => _sn;
            private set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged(nameof(Sn));
                }
            }
        }

        public string 定子检测Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(定子检测Status));
                }
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn,bool enterok, bool enterng,string status)
        {
            Req = req;
            Resp = resp;
            Sn = sn;
            EnterOk = enterok;
            EnterNg = enterng;
            定子检测Status= status;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}