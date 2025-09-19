using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SmapIt.Core;
using SmapIt.Utils;

namespace SmapIt.App
{
    internal class runGame
    {
        private const string LOG_IDENT = "App:RunGame";

        // URLs
        private const string smapitApiUrl = "https://api.github.com/repos/studzt/SmapIt/releases/latest";
        private const string smapiApiUrl = "https://api.github.com/repos/Pathoschild/SMAPI/releases/latest";

        private readonly Translator translator = new Translator();
        private readonly SettingsManager SettingsManager = new SettingsManager();

        bool loadProfile(string smapiPath, string profile)
        {
            AppCore.Logger.WriteLine(LOG_IDENT, $"Loading profile: {profile}");
            string? profilePath = profileManager.GetProfilePath(profile);
            if (string.IsNullOrEmpty(profilePath))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Failed to get profile path.");
                return false;
            }

            string? smapiDirectory = Path.GetDirectoryName(smapiPath);
            if (string.IsNullOrEmpty(smapiDirectory))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Failed to get SMAPI directory.");
                return false;
            }

            string modsFolder = Directory.CreateDirectory(Path.Combine(smapiDirectory, "Mods")).FullName;
            Directory.Delete(modsFolder, true);

            modsFolder = Directory.CreateDirectory(Path.Combine(smapiDirectory, "Mods")).FullName;

            foreach (string file in Directory.GetFiles(profilePath, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(profilePath, file);
                string destinationPath = Path.Combine(modsFolder, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? modsFolder);
                File.Copy(file, destinationPath, true);
            }

            AppCore.Logger.WriteLine(LOG_IDENT, $"Profile loaded successfully.");
            return true;
        }
        
        public async Task run(string smapiPath)
        {
            if (!File.Exists(smapiPath))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Invalid SMAPI path: {smapiPath}");
                translator.print("start_types.run.invalid_smapi_path");
                Console.ReadLine();
                return;
            }

            string? smapiDir = Path.GetDirectoryName(smapiPath);
            if (string.IsNullOrEmpty(smapiDir))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Invalid SMAPI path: {smapiPath}");
                translator.print("start_types.run.invalid_smapi_path");
                Console.ReadLine();
                return;
            }

            string smapitShortcut = Path.Combine(smapiDir, "SmapIt.lnk");
            if (!File.Exists(smapitShortcut))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Invalid SmapIt path: {smapitShortcut}");
                translator.print("start_types.run.smapit_not_found");
                Console.ReadLine();
                return;
            }

            await checkUpdates(smapiPath, smapitShortcut);
            Console.WriteLine("\n--------------------------\n");

            if (Convert.ToBoolean(SettingsManager.Settings["always_ask_profile"]))
            {
                while (true)
                {
                    string[] profileList = profileManager.GetProfiles();

                    if (profileList.Length <= 0)
                    {
                        translator.print("options.profiles.any_profile");
                        continue;
                    }

                    translator.print("options.profiles.choose_profile");
                    for (int i = 0; i < profileList.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}) {profileList[i]}");
                    }

                    Console.Write("\n> ");
                    string? input = Console.ReadLine();

                    if (int.TryParse(input, out int choice))
                    {
                        if (choice >= 1 && choice <= profileList.Length)
                        {
                            string selectedProfile = profileList[choice - 1];
                            translator.print("start_types.run.loading_profile");
                            bool result = loadProfile(smapiPath, selectedProfile);
                            if (!result)
                            {
                                translator.print("start_types.run.profile_load_error");
                                Console.ReadLine();
                                return;
                            }

                            translator.print("options.profiles.load_success");
                            Console.WriteLine("\n--------------------------\n");
                            break;
                        }
                        else
                        {
                            translator.print("options.profiles.invalid_choice");
                            continue;
                        }
                    }
                    else
                    {
                        translator.print("options.profiles.invalid_choice");
                        continue;
                    }
                }
            }
            else if (!Convert.ToBoolean(SettingsManager.Settings["always_ask_profile"])
                     && Convert.ToString(SettingsManager.Settings["default_profile"]) != "none")
            {
                translator.print("start_types.run.loading_profile");
                string? profile = Convert.ToString(SettingsManager.Settings["default_profile"]);
                if (string.IsNullOrEmpty(profile))
                {
                    translator.print("start_types.run.profile_load_error");
                    Console.ReadLine();
                    return;
                }

                bool success = loadProfile(smapiPath, profile);
                if (!success)
                {
                    translator.print("start_types.run.profile_load_error");
                    Console.ReadLine();
                    return;
                }

                translator.print("options.profiles.load_success");
                Console.WriteLine("\n--------------------------\n");
            }
            else
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Any profile is set as default and always ask profile option is not enabled.");
            }

            AppCore.Logger.WriteLine(LOG_IDENT, $"Starting SMAPI at: {smapiPath}");
            launchGame(smapiPath);
        }

        private async Task checkUpdates(string smapiPath, string smapitShortcut)
        {
            AppCore.Logger.WriteLine(LOG_IDENT, $"Starting update checking");

            if (!await isInternetAvailable())
            {
                translator.print("start_types.run.github_not_available");
                AppCore.Logger.WriteLine(LOG_IDENT, "Github not avaiable, skipping updates.");
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SmapIt");

            if (getSetting("auto_update_smapit"))
            {
                await updateSmapIt(client, smapitShortcut);
                Console.WriteLine();
            }

            if (getSetting("auto_update_smapi"))
            {
                await updateSmapi(client, smapiPath);
            }
        }

        private async Task updateSmapIt(HttpClient client, string smapitShortcut)
        {
            AppCore.Logger.WriteLine(LOG_IDENT, "Checking for SmapIt updates...");
            translator.print("start_types.run.smapit_update_check");

            var response = client.GetAsync(smapitApiUrl).Result;
            if (!response.IsSuccessStatusCode)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Github API did not return success status code: {response}");
                translator.print("start_types.run.error_smapit");
                return;
            }

            var releaseData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var latestVersion = parseVersion(releaseData["tag_name"]?.ToString());
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

            if (latestVersion <= currentVersion)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"SmapIt is updated: {currentVersion} - {latestVersion}");
                translator.print("start_types.run.smapit_updated");
                return;
            }

            AppCore.Logger.WriteLine(LOG_IDENT, $"SmapIt update avaiable: {currentVersion} -> {latestVersion}");
            translator.printFormatted("start_types.run.smapit_avaiable", new()
            {
                { "version", currentVersion.ToString() },
                { "newVersion", latestVersion.ToString() }
            });

            try
            {
                var SmapItUpdater = new SmapItUpdater();
                await SmapItUpdater.Update(client, smapitShortcut);
            }
            catch(Exception ex)
            {
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                translator.print("start_types.run.smapit_error");
                Console.ReadLine();
            }
        }

        private async Task updateSmapi(HttpClient client, string smapiPath)
        {
            AppCore.Logger.WriteLine(LOG_IDENT, "Checking for SMAPI updates...");
            translator.print("start_types.run.check_smapi");

            var response = await client.GetAsync(smapiApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Github API did not return success status code: {response}");
                translator.print("start_types.run.github_not_available");
                return;
            }

            var releaseData = JObject.Parse(await response.Content.ReadAsStringAsync());
            var latestVersion = parseVersion(releaseData["tag_name"]?.ToString());

            var fileInfo = FileVersionInfo.GetVersionInfo(smapiPath);
            if (fileInfo?.FileVersion == null)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Failed to get SMAPI current version.");
                translator.print("start_types.run.smapi_error");
                return;
            }

            var currentVersion = new Version(fileInfo.FileVersion);
            if (latestVersion <= currentVersion)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"SMAPI is updated: {currentVersion} - {latestVersion}");
                translator.print("start_types.run.smapi_updated");
                return;
            }

            translator.printFormatted("start_types.run.smapi_avaiable", new()
            {
                { "version", currentVersion.ToString() },
                { "newVersion", latestVersion.ToString() }
            });

            var parentDir = Directory.GetParent(smapiPath)?.FullName;
            if (string.IsNullOrEmpty(parentDir))
            {
                translator.print("start_types.run.error_update_smapi");
                return;
            }

            var smapiUpdater = new SmapiUpdater(parentDir);
            if (!await smapiUpdater.InstallSmapi())
            {
                translator.print("start_types.run.error_update_smapi");
            }
        }

        private void launchGame(string smapiPath)
        {
            translator.print("start_types.run.starting");
            Console.WriteLine("\n--------------------------\n");

            try
            {
                var processName = Path.GetFileNameWithoutExtension(smapiPath);
                if (Process.GetProcessesByName(processName).Length > 0)
                {
                    AppCore.Logger.WriteLine(LOG_IDENT, "SMAPI is already running.");
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

                AppCore.Logger.WriteLine(LOG_IDENT, "SMAPI started.");
                translator.print("start_types.run.success");

                Console.WriteLine();

                if (process != null && getSetting("hide_smapi_console"))
                {
                    process.OutputDataReceived += (_, args) => printConsole(args.Data, ConsoleColor.DarkYellow);
                    process.BeginOutputReadLine();

                    process.ErrorDataReceived += (_, args) => printConsole(args.Data, ConsoleColor.Red);
                    process.BeginErrorReadLine();

                    try { process.WaitForInputIdle(5000); } catch { }

                    waitForSmapiWindow(process);
                }

                AppCore.Logger.WriteLine(LOG_IDENT, "Stardew valley started successfully. Exiting with code 0.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error at trying to start SMAPI:");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                translator.print("start_types.run.error_start");
                Console.ReadLine();
            }
        }

        private void waitForSmapiWindow(Process process)
        {
            bool warned = false;
            int waited = 0;
            const int warnAfter = 10000;
            const int interval = 100;

            while (process.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(interval);
                waited += interval;
                process.Refresh();

                if (!warned && waited >= warnAfter)
                {
                    translator.print("start_types.run.smapi_warning");
                    warned = true;
                }

                if (process.HasExited)
                {
                    AppCore.Logger.WriteLine(LOG_IDENT, "SMAPI closed before starting StardewValley.exe");
                    Console.WriteLine("\n--------------------------\n");
                    translator.print("start_types.run.smapi_closed");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
        }

        // Utils
        private void printConsole(string? data, ConsoleColor color)
        {
            if (!string.IsNullOrEmpty(data))
            {
                Console.ForegroundColor = color;
                Console.WriteLine(data);
                Console.ResetColor();
            }
        }

        private bool getSetting(string key)
        {
            return Convert.ToBoolean(SettingsManager.Settings[key]);
        }

        private Version parseVersion(string? versionStr)
        {
            return Version.TryParse(versionStr?.TrimStart('v'), out var version)
                ? version
                : new Version(0, 0, 0, 0);
        }

        private async Task<bool> isInternetAvailable()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync("https://github.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}