using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GameLib
{
    public static class IniConfig
    {
        private const int MAXLEN = 255;
        private static StringBuilder mStringBuiler;  

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def_value, StringBuilder retval, int size, string filePath);

        static IniConfig()
        {
            // 정적생성자 내용이 필요한 경우 이곳에 기입
            mStringBuiler = new StringBuilder(MAXLEN);
        }

        /// <summary>
        /// Write xxx.ini file 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="value"></param>
        /// <param name="filePath"></param>
        public static void IniFileWrite(string section, string key, string value, string filePath)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }

        /// <summary>
        /// Read xxx.ini file
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="filePath"></param>
        /// <param name="def_value"></param>
        /// <param name="size"></param>
        public static string IniFileRead(string section, string key, string value, string filePath)
        {
            var ret = GetPrivateProfileString(section, key, value, mStringBuiler, MAXLEN, filePath);
            return mStringBuiler.ToString().Trim();
        }

    }
}
