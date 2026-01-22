using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FJY_Print
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(processName);
            //如果该数组长度大于1，说明多次运行
            if (processes.Length > 1)
            {
                MessageBox.Show("程序已运行，不能再次打开！");
                Environment.Exit(1);
            }
            else
            {
                // 中文授权示例
                if (!HslCommunication.Authorization.SetAuthorizationCode("95057912-579c-42d1-ad31-eb598f73706f"))
                {
                    //MessageBox.Show("授权失败！当前程序只能使用8小时！");
                    return; // 激活失败就退出系统
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
        }
    }
}
