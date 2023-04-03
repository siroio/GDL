using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDL
{
    internal class LoadFIle
    {
        public static List<string> Load(string path, params string[] sep)
        {
            using var reader = new StreamReader(path);
            string? line;
            List<string> lines = new List<string>();
            while ((line = reader.ReadLine()) != null)
            {
                foreach (var text in line.Trim().Split(sep, StringSplitOptions.RemoveEmptyEntries))
                {
                    lines.Add(text);
                }
            }
            return lines;
        }
    }
}
