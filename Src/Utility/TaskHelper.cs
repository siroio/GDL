using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDL
{
    public static class TaskHelper
    {
        public static Process GetProcess(string filename, string args = "", bool useShellExecute = false, bool createNoWindow = true)
        {
            var process = new Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = useShellExecute;
            process.StartInfo.CreateNoWindow = createNoWindow;
            return process;
        }
    }
}
