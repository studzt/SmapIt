using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using main.classes;
using Newtonsoft.Json.Linq;

namespace main.app
{
    internal class runGame
    {
        // Constants
        private const string smapitApiUrl = "https://api.github.com/repos/studzt/SmapIt/releases/latest";
        private const string smapiApiUrl = "https://api.github.com/repos/Pathoschild/SMAPI/releases/latest";

        // Fields
        private readonly string updaterPath;
        private readonly Translator translator = new Translator();
        private readonly settingsManager settingsManager = new settingsManager();

        // Constructor
        public runGame()
        {
            #if DEBUG
                updaterPath = Path.Combine("..", "..", "..", "SmapitUpdater.exe");
            #else
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                updaterPath = Path.Combine(exeDir, "SmapitUpdater.exe");
            #endif
        }

        // Main
        public async Task run(string smapiPath)
        {
            if (!File.Exists(smapiPath))
            {
                translator.print("start_types.run.invalid_smapi_path");
                return;
            }

            string? smapiDirectory = Path.GetDirectoryName(smapiPath);
            if (smapiDirectory == null)
            {
                translator.print("start_types.run.invalid_smapi_path");
                return;
            }

            string smapitShortcut = Path.Combine(smapiDirectory, "SmapIt.lnk");
            if (!File.Exists(smapitShortcut))
            {
                translator.print("start_types.run.smapit_not_found");
                return;
            }

            string quotedSmapitShortcut = $"\"{smapitShortcut}\"";
            await update(smapiPath, quotedSmapitShortcut);

            Console.WriteLine("\n--------------------------\n");

            launchGame(smapiPath);
        }

        // Handles updating logic
        private async Task update(string smapiPath, string smapitShortcut)
        {
            bool internetAvaiable = await checkInternetConnection();
            if (!internetAvaiable)
            {
                translator.print("start_types.run.github_not_available");
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SmapIt");

            if (getSetting("auto_update_smapit"))
            {
                updSmapIt(client, smapitShortcut);
                Console.WriteLine();
            }

            if (getSetting("auto_update_smapi"))
            {
                await updSmapi(client, smapiPath);
            }
        }

        // Updates the SmapIt app if needed
        private void updSmapIt(HttpClient client, string smapitShortcut)
        {
            translator.print("start_types.run.smapit_update_check");

            var response = client.GetAsync(smapitApiUrl).Result;
            if (!response.IsSuccessStatusCode)
            {
                translator.print("start_types.run.error_smapit");
                return;
            }

            var releaseData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var latestVersion = parseVersion(releaseData["tag_name"]?.ToString());
            var currentVersion = Assembly.GetExecutingAssembly()?.GetName().Version ?? new Version(0, 0, 0, 0);

            if (latestVersion <= currentVersion)
            {
                translator.print("start_types.run.smapit_updated");
                return;
            }

            translator.printFormatted("start_types.run.smapit_avaiable", new()
            {
                { "version", currentVersion.ToString() },
                { "newVersion", latestVersion.ToString() }
            });

            try
            {
                string currentProcessId = Process.GetCurrentProcess().Id.ToString();

                // Clean temporary folder
                string tempPath = Path.Combine(Path.GetTempPath(), "SmapIt");
                handleTempDir(tempPath);

                // Copy updater
                string tempUpdaterPath = Path.Combine(tempPath, "Updater.exe");
                File.Copy(updaterPath, tempUpdaterPath, overwrite: true);

                // Start updater
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempUpdaterPath,
                    Arguments = $"{currentProcessId} {translator.getLanguage()} {smapitShortcut}",
                    UseShellExecute = true
                });

                Environment.Exit(0);
            }
            catch
            {
                translator.print("start_types.run.smapit_error");
                Console.ReadLine();
            }
        }

        // Updates SMAPI if needed
        private async Task updSmapi(HttpClient client, string smapiPath)
        {
            translator.print("start_types.run.check_smapi");

            var response = client.GetAsync(smapiApiUrl).Result;
            if (!response.IsSuccessStatusCode)
            {
                translator.print("start_types.run.github_not_available");
                return;
            }

            var releaseData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var latestVersion = parseVersion(releaseData["tag_name"]?.ToString());

            var fileInfo = FileVersionInfo.GetVersionInfo(smapiPath);
            if (fileInfo?.FileVersion == null)
            {
                translator.print("start_types.run.smapi_error");
                return;
            }

            var currentVersion = new Version(fileInfo.FileVersion);
            if (latestVersion <= currentVersion)
            {
                translator.print("start_types.run.smapi_updated");
                return;
            }

            translator.printFormatted("start_types.run.smapi_avaiable", new()
            {
                { "version", currentVersion.ToString() },
                { "newVersion", latestVersion.ToString() }
            });

            var parentDirectory = Directory.GetParent(smapiPath);
            if (parentDirectory?.FullName == null)
            {
                translator.print("start_types.run.error_update_smapi");
                return;
            }

            var smapiInstaller = new smapiManager(parentDirectory.FullName);
            bool success = await smapiInstaller.installSmapi();
            if (!success)
                translator.print("start_types.run.error_update_smapi");
        }

        // Launches the SMAPI executable
        private void launchGame(string smapiPath)
        {
            translator.print("start_types.run.starting");
            Console.WriteLine("\n--------------------------\n");
            try
            {
                var processName = Path.GetFileNameWithoutExtension(smapiPath);

                var existingProcesses = Process.GetProcessesByName(processName);
                if (existingProcesses.Length > 0)
                {
                    translator.print("start_types.run.already_running");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = smapiPath,
                    WorkingDirectory = Path.GetDirectoryName(smapiPath),
                    UseShellExecute = !getSetting("hide_smapi_console"),
                    CreateNoWindow = getSetting("hide_smapi_console"),
                    RedirectStandardOutput = getSetting("hide_smapi_console"),
                    RedirectStandardError = getSetting("hide_smapi_console")
                };

                var process = Process.Start(startInfo);
                Console.WriteLine();
                translator.print("start_types.run.success");

                if (process != null && getSetting("hide_smapi_console"))
                {
                    // Redirect output to console
                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine(args.Data);
                            Console.ResetColor();
                        }
                    };
                    process.BeginOutputReadLine();

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(args.Data);
                            Console.ResetColor();
                        }
                    };
                    process.BeginErrorReadLine();

                    try { process.WaitForInputIdle(5000); } catch { }

                    bool warned = false;
                    int waited = 0;
                    const int warningMs = 10000;
                    const int intervalMs = 100;

                    while (process.MainWindowHandle == IntPtr.Zero)
                    {
                        Thread.Sleep(intervalMs);
                        waited += intervalMs;
                        process.Refresh();

                        if (!warned && waited >= warningMs)
                        {
                            translator.print("start_types.run.smapi_warning");
                            warned = true;
                        }

                        if (process.HasExited)
                        {
                            translator.print("start_types.run.smapi_closed");
                            Environment.Exit(1);
                        }
                    }
                }

                Environment.Exit(0);
            }

            catch (Exception ex)
            {
                translator.print("start_types.run.error_start");
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        // Prepares a clean temp directory
        private void handleTempDir(string tempPath)
        {
            if (Directory.Exists(tempPath))
            {
                var dir = new DirectoryInfo(tempPath);
                foreach (var file in dir.GetFiles()) file.Delete();
                foreach (var subDir in dir.GetDirectories()) subDir.Delete(true);
            }
            else
            {
                Directory.CreateDirectory(tempPath);
            }
        }

        // Gets a boolean setting value from settings
        private bool getSetting(string key)
        {
            return Convert.ToBoolean(settingsManager.Settings[key]);
        }

        // Parses a version from a string
        private Version parseVersion(string? versionString)
        {
            return Version.TryParse(versionString?.TrimStart('v'), out var version)
                ? version
                : new Version(0, 0, 0, 0);
        }

        private async Task<bool> checkInternetConnection()
        {
            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                string URL = "https://github.com";
                HttpResponseMessage response = await httpClient.GetAsync(URL);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
