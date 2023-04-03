using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Threading;
using System.Linq;
using System.Reflection.Emit;

namespace GDL
{

    internal class Program
    {
        private static readonly Queue<string> DownloadQueue = new();
        private static readonly List<Task> RunningTask = new();
        private static readonly GalleryDL galleryDL = new();
        private static readonly ConsoleLabels labels = new();

        private static bool IsInputFile = false;
        private static int _concurrentDownloads = 0;


        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            var exePath = Path.Combine(Environment.CurrentDirectory, "gallery-dl.exe");
            var currentVersion = galleryDL.GetVersionFromExe(exePath);
            await galleryDL.CheckAndUpdate(currentVersion);
            var config = GetConfig();


            labels.Position(0, 1);
            labels.Add("TITLE", "URL\t|\tSTATUS");

            if (args.Contains("-list"))
            {
                var pathIndex = Array.IndexOf(args, "-list") + 1;
                if (pathIndex >= args.Length)
                {
                    return;
                }

                var path = args[pathIndex];
                if (!Path.IsPathRooted(path))
                {
                    path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
                }
                Console.WriteLine(path);
                var load = LoadFIle.Load(path, "\r\n", "\n", "\r", ",", " ");
                foreach (var url in load)
                {
                    if (DownloadQueue.Contains(url))
                    {
                        continue;
                    }
                    DownloadQueue.Enqueue(url);
                    labels.Add(url, $"{url}\tStandby");
                }
                IsInputFile = true;
            }

            var downloadSemaphore = new SemaphoreSlim(config.downloadCount);
            while (true)
            {

                if (!IsInputFile)
                {
                    foreach (var url in UserInputUrl())
                    {
                        lock (DownloadQueue)
                            DownloadQueue.Enqueue(url);

                        lock (labels)
                            labels.Add(url, $"{url}\tStandby");
                    }
                }

                IsInputFile = false;
                while (DownloadQueue.Count > 0 && RunningTask.Count < config.downloadCount)
                {
                    var urlToDownload = DownloadQueue.Dequeue();
                    RunningTask.Add(StartDownloadTask(urlToDownload, config.args));
                }
            }

        }

        private static async Task StartDownloadTask(string url, string customArgs)
        {
            try
            {
                Interlocked.Increment(ref _concurrentDownloads);
                labels.SetText(url, $"{url}\tDownload");
                await galleryDL.DownloadAsync(url, customArgs);
                labels.SetText(url, $"{url}\tDone");
                labels.WriteText();
            }
            finally
            {
                Interlocked.Decrement(ref _concurrentDownloads);
            }
        }

        private static string[] UserInputUrl()
        {
            RunningTask.RemoveAll(task => task.IsCompleted);
            Console.Clear();
            labels.WriteText();
            Console.Write("Enter URL to download (or 'q' to quit): ");
            var urls = Console.ReadLine()?.Trim().Split(" ");

            if (urls == null || urls.Length == 0)
            {
                return Array.Empty<string>();
            }

            if (urls[0] == "q" || urls[0] == "quit")
            {
                return Array.Empty<string>();
            }

            foreach (var url in urls)
            {
                if (string.IsNullOrEmpty(url) || !RegexUtility.Match(url, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"))
                {
                    return Array.Empty<string>();
                }
            }

            return urls;
        }

        public static (string args, int downloadCount) GetConfig()
        {
            string args = "CustomArgs";
            string dCount = "DownloadCount";

            InIFile.Path = Path.Combine(Environment.CurrentDirectory, "cnf.ini");
            var config = InIFile.ReadValue("Config", args, dCount);

            string argString = string.Join(" ", config[args]);
            bool isArgs = RegexUtility.Match(argString, @"^(-{1,2}\w+\s+)+(-{1,2}\w+)?$") || argString == "NONE";

            if (!isArgs || !int.TryParse(config[dCount], out var result))
            {
                Console.WriteLine("Arguments must be separated by spaces.");
                return (string.Empty, 0);
            }

            Console.WriteLine("Loaded Config.");
            return (args, result);
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Console.Clear();
            Console.WriteLine("Exiting...");
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName == "gallery-dl")
                {
                    process.Kill();
                }
            }
            e.Cancel = false;
        }
    }
}