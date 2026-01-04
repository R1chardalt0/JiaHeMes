using System;
using System.Security.Cryptography;
using System.Text;

namespace DeviceManage.Helpers
{
    /// <summary>
    /// MD5加密工具类
    /// </summary>
    public static class MD5Helper
    {
        /// <summary>
        /// 对字符串进行MD5加密
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <returns>MD5加密后的字符串（32位大写）</returns>
        public static string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // 将字节数组转换为32位大写十六进制字符串
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 验证密码是否匹配
        /// </summary>
        /// <param name="input">输入的原始密码</param>
        /// <param name="hashedPassword">已加密的密码</param>
        /// <returns>是否匹配</returns>
        public static bool Verify(string input, string hashedPassword)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            string encryptedInput = Encrypt(input);
            return encryptedInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}

