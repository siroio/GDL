using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDL.Src.Utility;

namespace GDL.Src
{
    internal class Downloader
    {
        // ダウンロード待ち
        private static readonly Queue<string> DownloadQueue = new();
        // 実行中ダウンロード
        private static readonly List<Task> RunningTask = new();
        // UIラベル
        private static readonly ConsoleLabels labels = new();
        // GalleryDLラッパー
        private static readonly GalleryDL galleryDL = new();
        // 有効セパレータ
        private static readonly string[] SeparatorList = { "\r\n", "\n", "\r", ",", " " };
        // 現在のダウンロード実行数
        private static int CurrentDownload = 0;
        // コンフィグ
        private (string args, int count) config = (string.Empty, 0);
        private bool LoadURLText = false;
        private string textPath = string.Empty;

        // 終了処理
        public event Action? ExitEvent;

        public async void Init(Action exitEvent)
        {
            await galleryDL.CheckAndUpdate(
                galleryDL.GetVersionFromExe(Path.Combine(Environment.CurrentDirectory, "gallery-dl.exe"))
            );
            if (exitEvent == null) throw new ArgumentNullException("ExitEvent is null");
            config = GetConfig();
            labels.Position(0, 1);
            labels.Add("TITLE", "URL\t|\t\tSTATUS");
            ExitEvent += exitEvent;
        }

        public void SetListDownload(string[] args)
        {
            var argList = ArgsHelper.GetArgs(args, "-list");
            if (argList.Count > 0)
            {
                textPath = PathHelper.GetAbsolute(argList["-list"]);
                LoadURLText = true;
                var loadUrls = LoadFile.Load(textPath, SeparatorList);
                DownloadQueue.AddAll(loadUrls, (url) => !DownloadQueue.Contains(url));
                DownloadQueue.ForEach((url) => labels.Add(url, $"{url}\tStandby"));
            }
        }

        public void StartDownloadLoop()
        {
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
                        ExitEvent?.Invoke();
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
                while (DownloadQueue.Count > 0 && RunningTask.Count < config.count)
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

        /// <summary>
        /// ユーザーの入力待ち
        /// </summary>
        /// <param name="input"></param>
        private static string[] UserInputUrl(string[] input)
        {
            var nonNullOrEmpty = input.All((i) => !string.IsNullOrEmpty(i));
            var isURLS = RegexHelper.MatchArray(input, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            return nonNullOrEmpty && isURLS && input.Length > 0 ? input : Array.Empty<string>();
        }


        /// <summary>
        /// コンフィグの取得
        /// </summary>
        private (string args, int downloadCount) GetConfig()
        {
            string args = "CustomArgs";
            string dCount = "DownloadCount";
            var filePath = Path.Combine(Environment.CurrentDirectory, "cnf.ini");

            if (!File.Exists(filePath))
            {
                MsgBoxHelper.ShowMessage("Can't find cnf.ini", "Error");
                ExitEvent?.Invoke();
                return ("", -1);
            }

            InIFile.Path = filePath;
            var config = InIFile.ReadValue("Config", args, dCount);

            string argString = string.Join(" ", config[args]);
            bool isArgs = RegexHelper.Match(argString, @"^(-{1,2}\w+\s+)+(-{1,2}\w+)?$") || argString == "NONE";

            if (!isArgs || !int.TryParse(config[dCount], out var result))
            {
                Console.WriteLine("Arguments must be separated by spaces.");
                return (string.Empty, 0);
            }

            Console.WriteLine("Loaded Config.");
            return (args, result);
        }
    }
}
