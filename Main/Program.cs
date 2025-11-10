using SmapIt.App;
using SmapIt.Core;
using SmapIt.Utils;
using Sentry;

class Program
{
    private const string LOG_IDENT = "SmapIt::Root";
    static async Task Main(string[] args)
    {
        var mutex = new AsyncMutex("SmapIt_Main");
        if (!await TryAcquireMutexAsync(mutex, TimeSpan.FromSeconds(5)))
        {
            Console.WriteLine("SmapIt is already running.\nPress ENTER to close...");
            Console.ReadLine();
            Environment.Exit(0);
        }

        try
        {
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

            AppCore.Logger.Initialize();
            AppCore.tempLogger.Initialize(true);

            AppCore.Logger.WriteLine(LOG_IDENT, $"Running in: {AppContext.BaseDirectory}");
            AppCore.Logger.WriteLine(LOG_IDENT, $"Windows version: {System.Environment.OSVersion}");

            AppCore.tempLogger.WriteLine(LOG_IDENT, $"Running in: {AppContext.BaseDirectory}");
            AppCore.tempLogger.WriteLine(LOG_IDENT, $"Windows version: {System.Environment.OSVersion}");

            var translator = new Translator();

            string tempPath = Path.Combine(Path.GetTempPath(), "SmapIt");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            if (args.Length > 0)
            {
                await ProcessArgumentsAsync(args, translator);
                return;
            }

            await new App().run();
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            Console.WriteLine("Unexpected error. Check logs for more information");
            AppCore.Logger.WriteLine(LOG_IDENT, "FATAL ERROR");
            AppCore.Logger.WriteException(LOG_IDENT, ex);
        }
        finally
        {
            Console.WriteLine("\nPress ENTER to leave...");
            Console.ReadLine();

            await mutex.ReleaseAsync();
        }
    }

    private static async Task ProcessArgumentsAsync(string[] args, Translator translator)
    {
        string command = args[0];

        switch (command)
        {
            case "--start":
                string smapiPath = "";

                if (args.Length >= 3 && args[1] == "--path")
                {
                    string pathCandidate = args[2];

                    if (File.Exists(pathCandidate) && Path.GetFileName(pathCandidate) == "StardewModdingAPI.exe")
                    {
                        smapiPath = pathCandidate;
                    }
                    else
                    {
                        translator.print("invalid_smapi_path");
                        Console.WriteLine(args[1]);
                        return;
                    }
                }

                await new runGame().run(smapiPath);
                break;

            default:
                AppCore.Logger.WriteLine(LOG_IDENT, $"Unknown argument: {command}");
                break;
        }
    }

    private static async Task<bool> TryAcquireMutexAsync(AsyncMutex mutex, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await mutex.AcquireAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
