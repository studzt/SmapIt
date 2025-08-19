using main.classes;
using main.app;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Mutex
            bool createdNew;
            var mutex = new Mutex(true, "Smapit", out createdNew);

            if (!createdNew)
            {
                Console.WriteLine("SmapIt is already running.");
                return;
            }

            // Cleaning temp folder
            string tempPath = Path.Combine(Path.GetTempPath(), "SmapIt");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            // Utils
            var translator = new Translator();

            // Args
            if (args.Length > 0)
            {
                string command = args[0];

                switch (command)
                {
                    case "--start":
                        string path = "";

                        // Checks if path is valid
                        if (args.Length >= 3 && args[1] == "--path")
                        {
                            string smapiPath = args[2];

                            if (File.Exists(smapiPath) && Path.GetFileName(smapiPath) == "StardewModdingAPI.exe")
                            {
                                path = args[2];
                            }
                            else
                            {
                                translator.print("invalid_smapi_path");
                                Console.WriteLine(args[1]);
                                return;
                            }
                        }

                        await new runGame().run(path);
                        break;
                }

                return;
            }

            // Main
            await new App().run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            Console.WriteLine("\nPress ENTER to leave...");
            Console.ReadLine();
        }
    }
}
