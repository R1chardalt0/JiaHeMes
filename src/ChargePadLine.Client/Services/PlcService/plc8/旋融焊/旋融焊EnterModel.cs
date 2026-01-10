using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc8.旋融焊
{
    public class 旋融焊EnterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _enterOk;
        private bool _enterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 旋融焊EnterReq
        {
            get => _req;
            private set
            {
                _req = value;
                OnPropertyChanged(nameof(旋融焊EnterReq));
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 旋融焊EnterResp
        {
            get => _resp;
            private set
            {
                _resp = value;
                OnPropertyChanged(nameof(旋融焊EnterResp));
            }
        }

        public bool 旋融焊EnterNg
        {
            get => _enterNg;
            private set
            {
                _enterNg = value;
                OnPropertyChanged(nameof(旋融焊EnterNg));
            }
        }


        public bool 旋融焊EnterOk
        {
            get => _enterOk;
            private set
            {
                _enterOk = value;
                OnPropertyChanged(nameof(旋融焊EnterOk));
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 旋融焊EnterSn
        {
            get => _sn;
            private set
            {
                _sn = value;
                OnPropertyChanged(nameof(旋融焊EnterSn));
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool enterok, bool enterng)
        {
            // 强制更新，即使值相同也触发PropertyChanged
            旋融焊EnterReq = req;
            旋融焊EnterResp = resp;
            旋融焊EnterSn = sn;
            旋融焊EnterOk = enterok;
            旋融焊EnterNg = enterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
