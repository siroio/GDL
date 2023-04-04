using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GDL
{
    public static class ArgsHelper
    {
        public static Dictionary<string, string> GetArgs(string[] args, params string[] keyArgs)
        {
            var result = new Dictionary<string, string>();
            foreach (string arg in keyArgs)
            {
                var pathIndex = Array.IndexOf(args, arg) + 1;
                if (pathIndex >= args.Length)
                {
                    return result;
                }
                result.Add(arg, args[pathIndex]);
            }

            return result;
        }
    }
}
