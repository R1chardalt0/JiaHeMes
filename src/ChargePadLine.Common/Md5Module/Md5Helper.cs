using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Common.Md5Module
{
    public static class Md5Helper
    {
        public static string ToMd5(this string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(Encoding.Default.GetBytes(str + "@Webyao"));
            var md5Str = BitConverter.ToString(bytes).Replace("-", "");
            return md5Str;
        }
        //    public static string GetMd5Hash(string input)
        //     {
        //         using (var md5 = System.Security.Cryptography.MD5.Create())
        //         {
        //             var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        //             var hashBytes = md5.ComputeHash(inputBytes);
        //             var sb = new System.Text.StringBuilder();
        //             foreach (var hashByte in hashBytes)
        //             {
        //                 sb.Append(hashByte.ToString("X2"));
        //             }
        //             return sb.ToString();
        //         }
        //     } 
    }
}
