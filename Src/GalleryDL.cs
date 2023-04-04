using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GDL
{
    internal class GalleryDL
    {
        private const string Owner = "mikf";
        private const string Repo = "gallery-dl";
        private const string API_URL = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
        public string ExePath { get; private set; } = string.Empty;

        public async Task<string> GetLatestReleaseDownloadLink()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# console app");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

            var response = await httpClient.GetAsync(API_URL);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var release = JsonConvert.DeserializeObject<GitHubRelease>(responseContent);
            var asset = release?.Assets?.Find(x => x.Name.EndsWith(".exe"));

            return asset?.BrowserDownloadUrl ?? string.Empty;
        }

        public async Task CheckAndUpdate(string currentVersion)
        {
            var dlLink = await GetLatestReleaseDownloadLink();
            if (string.IsNullOrEmpty(dlLink))
            {
                Console.WriteLine("Failed to get the latest release download link.");
                return;
            }

            var latestVersion = GetVersionFromDownloadLink(dlLink);

            if (string.IsNullOrEmpty(currentVersion) || (latestVersion.CompareTo(currentVersion) > 0))
            {
                Console.WriteLine("Downloading the latest release...");
                await DownloadLatestRelease(dlLink);
            }
            else
            {
                Console.WriteLine("Your software is up to date.");
            }
        }

        public string GetVersionFromExe(string exePath)
        {
            ExePath = exePath;
            if (!File.Exists(exePath))
            {
                return string.Empty;
            }
            var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
            return versionInfo.FileVersion ?? string.Empty;
        }

        public string GetVersionFromDownloadLink(string downloadLink)
        {
            var regex = new Regex(@"/v(\d+\.\d+\.\d+)/gallery-dl\.exe$");
            var match = regex.Match(downloadLink);
            if (!match.Success)
            {
                return string.Empty;
            }

            return match.Groups[1].Value;
        }

        public async Task DownloadLatestRelease(string downloadLink)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(downloadLink);
            response.EnsureSuccessStatusCode();

            var fileName = Path.GetFileName(downloadLink);
            var filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            var fileSize = response.Content.Headers.ContentLength ?? -1;
            var totalBytesRead = 0L;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[4096];
            var bytesRead = 0;
            while (bytesRead >= 0)
            {
                bytesRead = await stream.ReadAsync(buffer);
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Downloading: {totalBytesRead}/{fileSize} bytes");
                if (totalBytesRead >= fileSize)
                {
                    break;
                }
            }

            Console.WriteLine($"\nDownloaded the latest release to {filePath}");

        }

        internal async Task<int> DownloadAsync(string url, string args)
        {
            var arg = $"{url} {args}";
            var proc = TaskHelper.GetProcess(ExePath, arg);
            proc.Start();
            await proc.WaitForExitAsync();
            return proc.ExitCode;
        }

        public class GitHubRelease
        {
            [JsonProperty("assets")]
            public List<GitHubAsset> Assets { get; set; } = new List<GitHubAsset>();
        }

        public class GitHubAsset
        {
            [JsonProperty("name")]
            public string Name { get; set; } = string.Empty;

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; } = string.Empty;
        }
    }
}
