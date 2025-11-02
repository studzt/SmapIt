using SmapIt.App;
using SmapIt.Core;
using SmapIt.Utils;

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
