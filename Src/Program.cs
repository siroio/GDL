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

    internal class Program
    {
        private static readonly Queue<string> DownloadQueue = new();
        private static readonly List<Task> RunningTask = new();
        private static readonly GalleryDL galleryDL = new();
        private static readonly ConsoleLabels labels = new();
        private static readonly string[] SeparatorList = { "\r\n", "\n", "\r", ",", " " };
        private static int CurrentDownload = 0;



        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (s, e) => ExitEvent();
            var exePath = Path.Combine(Environment.CurrentDirectory, "gallery-dl.exe");
            await galleryDL.CheckAndUpdate(galleryDL.GetVersionFromExe(exePath));
            var config = GetConfig();
            bool LoadURLText = false;
            string textPath = string.Empty;

            labels.Position(0, 1);
            labels.Add("TITLE", "URL\t|\t\tSTATUS");

            var argList = ArgsHelper.GetArgs(args, "-list");

            if (argList.Count > 0)
            {
                textPath = PathHelper.GetAbsolute(argList["-list"]);
                LoadURLText = true;
                var loadUrls = LoadFile.Load(textPath, SeparatorList);
                DownloadQueue.AddAll(loadUrls, (url) => !DownloadQueue.Contains(url));
                DownloadQueue.ForEach((url) => labels.Add(url, $"{url}\tStandby"));
            }

            while (true)
            {
                RunningTask.RemoveAll(task => task.IsCompleted);

                if (!LoadURLText)
                {
                    Console.Clear();
                    labels.WriteText();
                    Console.Write("Enter URL to download (or 'q' to quit): ");
                    var urls = Console.ReadLine()?.Trim().Split(SeparatorList, StringSplitOptions.RemoveEmptyEntries);

                    if (urls == null || urls.Length == 0)
                    {
                        continue;
                    }

                    if (urls[0] == "q" || urls[0] == "quit")
                    {
                        ExitEvent();
                        return;
                    }

                    foreach (var url in UserInputUrl(urls))
                    {
                        if (string.IsNullOrEmpty(url))
                        {
                            continue;
                        }

                        lock (DownloadQueue)
                            DownloadQueue.Enqueue(url);

                        lock (labels)
                            labels.Add(url, $"{url}\tStandby");
                    }
                }

                LoadURLText = false;
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
                Interlocked.Increment(ref CurrentDownload);
                labels.SetText(url, $"{url}\tDownload");
                await galleryDL.DownloadAsync(url, customArgs);
                labels.SetText(url, $"{url}\tDone");
                labels.WriteText();
            }
            finally
            {
                Interlocked.Decrement(ref CurrentDownload);
            }
        }

        private static string[] UserInputUrl(string[] input)
        {
            var nonNullOrEmpty = input.All((i) => !string.IsNullOrEmpty(i));
            var isURLS = RegexUtility.MatchArray(input, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            return nonNullOrEmpty && isURLS && input.Length > 0 ? input : Array.Empty<string>();
        }

        public static (string args, int downloadCount) GetConfig()
        {
            string args = "CustomArgs";
            string dCount = "DownloadCount";
            var filePath = Path.Combine(Environment.CurrentDirectory, "cnf.ini");

            if (!File.Exists(filePath))
            {
                MsgBoxHelper.ShowMessage("Can't find cnf.ini", "Error");
                ExitEvent();
                return ("", -1);
            }

            InIFile.Path = filePath;
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

        private static void ExitEvent()
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

            Environment.Exit(0);
        }
    }
}