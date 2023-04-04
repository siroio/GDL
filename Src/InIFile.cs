using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GDL
{
    public static class InIFile
    {
        // iniファイルのパス
        private static string path = @"C:\example.ini";
        public static string Path { get => path; set => path = value; }

        // iniファイルからキーと値のペアを取得
        public static string ReadValue(string section, string key)
        {
            var result = new StringBuilder(255);
            GetPrivateProfileString(section, key, string.Empty, result, 255, path);
            return result.ToString();
        }

        // iniファイルからキーと値のペアを取得
        public static Dictionary<string, string> ReadValue(string section, params string[] keys)
        {
            Dictionary<string, string> values = new();
            foreach (var key in keys.AsSpan())
            {
                values.Add(key, ReadValue(section, key));
            }
            return values;
        }

        // iniファイルにキーと値のペアを設定
        public static void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        // iniファイルの指定セクションのキーを全て取得
        public static string[] GetKeys(string section)
        {
            var result = new StringBuilder(255);
            GetPrivateProfileString(section, null, "", result, 255, path);
            var keys = result.ToString().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            return keys;
        }

        // iniファイルの指定セクションを全て取得
        public static string[] GetSections()
        {
            var result = new StringBuilder(255);
            GetPrivateProfileSectionNames(result, 255, path);
            var sections = result.ToString().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            return sections;
        }

        // Win32 APIの呼び出し
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string? key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileSectionNames(StringBuilder lpszReturnBuffer, int nSize, string lpFileName);
    }
}
