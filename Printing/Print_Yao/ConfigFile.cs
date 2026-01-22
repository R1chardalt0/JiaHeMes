using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NJCH_Station 
{
    class Config
    {
        public static string Path_Setting = Environment.CurrentDirectory + "\\Config\\Setting.ini";   //通讯设置
        public static string Path_Model = Environment.CurrentDirectory + "\\Config\\Model.ini";   //通讯设置

        public static string Path_PW = Environment.CurrentDirectory + "\\Pass";   //通讯设置


        public static void InitializePLCConfig()
        {
            // 检查配置目录是否存在，不存在则创建
            string configDir = Path.GetDirectoryName(Path_Setting);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            // 检查文件是否存在，不存在则写入默认PLC配置
            if (!File.Exists(Path_Setting))
            {
                // 写入默认IP和端口
                WritePrivateProfileString("PLC", "IP", "127.0.0.1", Path_Setting);
                WritePrivateProfileString("PLC", "Port", "502", Path_Setting);
            }
        }
        // 新增：读取PLC IP地址
        public static string GetPLCIP()
        {
            StringBuilder ip = new StringBuilder(255);
            GetPrivateProfileString("PLC", "IP", "", ip, 255, Path_Setting);
            return ip.ToString().Trim();
        }

        // 新增：读取PLC端口号（移除硬编码默认值）
        public static int GetPLCPort()
        {
            StringBuilder port = new StringBuilder(255);
            GetPrivateProfileString("PLC", "Port", "", port, 255, Path_Setting);
            return int.TryParse(port.ToString(), out int result) ? result : 0;
        }

        // 新增：保存PLC IP地址
        public static void SavePLCIP(string ip)
        {
            WritePrivateProfileString("PLC", "IP", ip, Path_Setting);
        }

        // 新增：保存PLC端口号
        public static void SavePLCPort(int port)
        {
            WritePrivateProfileString("PLC", "Port", port.ToString(), Path_Setting);
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);

        //获取ini文件中所有的section的name，注意，这些name不是以字符串格式返回，而是byte格式的数组
        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileSectionNames(byte[] buffer, int size, string filePath);

        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileStringA(string section, string key,
            string def, Byte[] retVal, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        public static void SetValue(string section, string key, string iValue, string Path)
        {
            WritePrivateProfileString(section, key, iValue, Path);
        }
        public static string ReadValue(string section, string key, string Path)
        {
            StringBuilder temp = new StringBuilder(255);
          
            int i = GetPrivateProfileString(section, key, "", temp, 255, Path);

            return temp.ToString();
        }
        public byte[] IniReadValues(string section, string key, string Path)
        {
            byte[] temp = new byte[255];
            int i = GetPrivateProfileString(section, key, "", temp, 255, Path);
            return temp;
        }

        //注意获取指定ini文件中的section的name放在一个ArrayList中返回
        public static ArrayList ReadSections(string Path)
        {
            byte[] buffer = new byte[65535];
            int rel = GetPrivateProfileSectionNames(buffer, buffer.GetUpperBound(0), Path);
            return Conver2ArrayList(rel, buffer);
        }

        public static ArrayList Conver2ArrayList(int rel, byte[] buffer)
        {
            ArrayList arrayList = new ArrayList();
            if (rel > 0)
            {
                int iCnt, iPos;
                string tmp;
                iCnt = 0; iPos = 0;
                for (iCnt = 0; iCnt < rel; iCnt++)
                {
                    if (buffer[iCnt] == 0x00)
                    {
                        tmp = System.Text.ASCIIEncoding.Default.GetString(buffer, iPos, iCnt - iPos).Trim();
                        iPos = iCnt + 1;
                        if (tmp != "")
                            arrayList.Add(tmp);
                    }
                }
            }
            return arrayList;
        }


        public static List<string> ReadKeys(string SectionName, string iniFilename)
        {
            List<string> result = new List<string>();
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(SectionName, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }
        /// 获取INI文件中指定节点(Section)中的所有条目(key=value形式)
        public static string[] INIGetAllItems(string iniFile, string section)
        {

            //返回值形式为 key=value,例如 Color=Red
            uint MAX_BUFFER = 32767;    //默认为32767
            string[] items = new string[0];      //返回值
            //分配内存
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));
            uint bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, iniFile);
            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {
                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
            Marshal.FreeCoTaskMem(pReturnedString);     //释放内存
            return items;
        }

        /*
3.实现”删“的功能
如果要删除整个”section1“节点，如下
WritePrivateProfileString(”section1“, null, null, @"C:\1.ini");
 
如果要删除”section1“下的”key1“，如下
WritePrivateProfileString(”section1“, "key1", null, @"C:\1.ini");
 
4.实现"改"的功能
如果我们要将”section1“下的"key1“的值改为"word“，其实它就是"增"，只是覆盖了原来的内容而已
WritePrivateProfileString(”section1“, "key1", ”word“, @"C:\1.ini");
 
5.实现 "查"的功能
如果要获取”section1“下的”key1“的内容，如下
GetPrivateProfileString("section1“, "key1", "FFFF", str1, 255, @"C:\1.ini");
*/
    }


}
