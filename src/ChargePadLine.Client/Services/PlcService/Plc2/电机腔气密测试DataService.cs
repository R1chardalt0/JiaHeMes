using System;
using System.ComponentModel;

namespace ChargePadLine.Client.Services.PlcService.Plc2
{
    /// <summary>
    /// 电机腔气密测试数据服务，用于存储和通知PLC数据变化
    /// </summary>
    public class 电机腔气密测试DataService : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private string _sn = string.Empty;

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

        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn)
        {
            Req = req;
            Resp = resp;
            Sn = sn;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}