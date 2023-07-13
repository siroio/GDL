using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using GDL.Src;
using GDL.Src.Utility;

namespace GDL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var downloader = new Downloader();
            Console.CancelKeyPress += (s, e) => ExitEvent();
            downloader.Init(ExitEvent);
            downloader.SetListDownload(args);
            downloader.StartDownloadLoop();
        }

        private static void ExitEvent()
        {
            Console.Clear();
            Console.WriteLine("Exiting...");
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName == "gallery-dl")
                {
                    process.Kill();
                }
            }

            Environment.Exit(0);
        }
    }
}