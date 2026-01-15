using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc8.旋融焊
{
    public class 旋融焊MasterModel : INotifyPropertyChanged
    {
        private bool _req;
        private bool _resp;
        private bool _masterOk;
        private bool _masterNg;
        private string _sn = string.Empty;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool 旋融焊MasterReq
        {
            get => _req;
            private set
            {
                _req = value;
                OnPropertyChanged(nameof(旋融焊MasterReq));
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public bool 旋融焊MasterResp
        {
            get => _resp;
            private set
            {
                _resp = value;
                OnPropertyChanged(nameof(旋融焊MasterResp));
            }
        }

        public bool 旋融焊MasterNg
        {
            get => _masterNg;
            private set
            {
                _masterNg = value;
                OnPropertyChanged(nameof(旋融焊MasterNg));
            }
        }


        public bool 旋融焊MasterOk
        {
            get => _masterOk;
            private set
            {
                _masterOk = value;
                OnPropertyChanged(nameof(旋融焊MasterOk));
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        public string 旋融焊MasterSn
        {
            get => _sn;
            private set
            {
                _sn = value;
                OnPropertyChanged(nameof(旋融焊MasterSn));
            }
        }



        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData(bool req, bool resp, string sn, bool masterok, bool masterng)
        {
            旋融焊MasterReq = req;
            旋融焊MasterResp = resp;
            旋融焊MasterSn = sn;
            旋融焊MasterOk = masterok;
            旋融焊MasterNg = masterng;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
