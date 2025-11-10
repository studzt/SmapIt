using Newtonsoft.Json.Linq;
using SmapIt.Utils;
using System.Diagnostics;
using Sentry;

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

                // Initialize Sentry
                SentrySdk.Init(options =>
                {
                    // A Sentry Data Source Name (DSN) is required.
                    // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                    // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
                    options.Dsn = "https://72c00da18e16d53bf259ba4062a9b52a@o4510337710555136.ingest.de.sentry.io/4510337733296208";

                    // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                    // This might be helpful, or might interfere with the normal operation of your application.
                    // We enable it here for demonstration purposes when first trying Sentry.
                    // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
                    options.Debug = false;

                    // This option is recommended. It enables Sentry's "Release Health" feature.
                    options.AutoSessionTracking = true;

                    // Set TracesSampleRate to 1.0 to capture 100%
                    // of transactions for tracing.
                    // We recommend adjusting this value in production.
                    options.TracesSampleRate = 0.2;
                });

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
                        SentrySdk.CaptureMessage("SmapIt did not closed after 15 seconds");
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
                    SentrySdk.CaptureException(ex);
                    translator.print("error");
                }
            }

            catch (OperationCanceledException)
            {
                Console.WriteLine("SmapIt Updater is already running.\nPress ENTER to leave.");
                Console.ReadLine();
            }

            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
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
                SentrySdk.CaptureMessage("Download URL not found");
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
                SentrySdk.CaptureException(ex);
                translator.print("start_types.run.error");
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
