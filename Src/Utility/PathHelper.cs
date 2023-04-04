using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDL.Src
{
    public static class PathHelper
    {
        public static string GetAbsolute(string path)
        {
            return Path.IsPathRooted(path) ?
                path :
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }
    }
}
