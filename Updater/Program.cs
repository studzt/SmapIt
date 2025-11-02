using Newtonsoft.Json.Linq;
using SmapIt.Utils;
using System.Diagnostics;

namespace main
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AsyncMutex Mutex = new AsyncMutex("SmapIt_Updater");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                await Mutex.AcquireAsync(cts.Token);

                if (args.Length < 2)
                {
                    Console.WriteLine("Missing arguments: expected [ProcessId] [LanguageCode] [smapitShortcut](optional)");
                    return;
                }

                string processIdArg = args[0];
                string languageCode = args[1];
                string smapitShortcut = args.Length > 2 && !string.IsNullOrWhiteSpace(args[2]) ? args[2] : "";

                Translator translator = new Translator(languageCode);
                translator.print("starting");

                // Checking things before updating
                if (checkProcess(processIdArg, out var smapitProcess) && smapitProcess != null)
                {
                    using var exitEvent = new AutoResetEvent(false);
                    smapitProcess.EnableRaisingEvents = true;
                    smapitProcess.Exited += (sender, e) =>
                    {
                        exitEvent.Set();
                    };

                    // Waiting main process to be closed
                    if (!smapitProcess.HasExited && !exitEvent.WaitOne(15000))
                    {
                        translator.print("smapit_closed_err");
                        return;
                    }
                }

                bool internetAvaiable = await checkInternetConnection();
                if (!internetAvaiable)
                {
                    translator.print("error");
                    Console.WriteLine("Network is not available.");
                    return;
                }

                // Updating
                try
                {
                    update(translator, smapitShortcut);
                }
                catch (Exception ex)
                {
                    translator.print("error");
                    Console.WriteLine(ex.Message);
                }
            }

            catch (OperationCanceledException)
            {
                Console.WriteLine("SmapIt Updater is already running.\nPress ENTER to leave.");
                Console.ReadLine();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("Press ENTER to leave...");
                Console.ReadLine();

                await Mutex.ReleaseAsync();
                await Mutex.DisposeAsync();
            }
        }

        private static bool checkProcess(string idString, out Process? process)
        {
            process = null;
            if (!int.TryParse(idString, out int pid))
                return false;

            try
            {
                process = Process.GetProcessById(pid);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void update(Translator translator, string smapitShortcut = "")
        {
            const string apiUrl = "https://api.github.com/repos/studzt/SmapIt/releases/latest";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SmapIt");

            var response = client.GetAsync(apiUrl).Result;
            if (!response.IsSuccessStatusCode)
            {
                translator.print("error");
                return;
            }

            var releaseData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var assets = releaseData["assets"] as JArray;

            string? downloadUrl = assets?
                .OfType<JObject>()
                .FirstOrDefault(a => a["name"]?.ToString().EndsWith(".exe") == true)?
                ["browser_download_url"]?.ToString();

            if (string.IsNullOrEmpty(downloadUrl))
            {
                translator.print("error");
                Console.WriteLine("Download URL not found.");
                return;
            }

            translator.print("download");

            string tempPath = Path.Combine(Path.GetTempPath(), "SmapIt");
            string installerPath = Path.Combine(tempPath, "SmapitInstaller.exe");
            Directory.CreateDirectory(tempPath);

            using (var downloadStream = client.GetStreamAsync(downloadUrl).Result)
            using (var fileStream = File.Open(installerPath, FileMode.Create))
            {
                downloadStream.CopyTo(fileStream);
            }

            translator.print("installing");
            try
            {
                var installerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = $"/verysilent /norestart"
                });

                installerProcess?.WaitForExit();
                translator.print("success");

                if (File.Exists(smapitShortcut))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = smapitShortcut,
                        UseShellExecute = true
                    });

                    Environment.Exit(0);
                }

                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                translator.print("start_types.run.error");
                Console.WriteLine(ex.Message);
            }
        }
        static async Task<bool> checkInternetConnection()
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
