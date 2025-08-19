using main.classes;

namespace main.app.menuOptions
{
    internal class Start
    {
        public async Task start()
        {
            // Classes
            settingsManager settingsManager = new settingsManager();
            Translator translator = new Translator();

            var installs = (settingsManager.Settings["_smapitInstalls"] as Newtonsoft.Json.Linq.JObject)?
                .ToObject<Dictionary<string, string>>() ?? new();

            if (installs.Count == 0)
            {
                translator.print("start_types.run.no_installations");
                Console.WriteLine();
                return;
            }

            if (installs.Count == 1)
            {
                var install = installs.First();
                var replacements = new Dictionary<string, string>
            {
                { "installName", install.Key },
                { "installDir", install.Value }
            };

                // Start
                string smapiPath = Path.Combine(install.Value, "StardewModdingAPI.exe");
                if (!File.Exists(smapiPath))
                {
                    translator.print("start_types.run.smapi_not_found");
                    Console.WriteLine();
                    return;
                }


                translator.printFormatted("start_types.run.start_run", replacements);
                await new runGame().run(smapiPath);
                Console.WriteLine("\n--------------------------\n");
            }

            if (installs.Count >= 2)
            {
                translator.print("start_types.run.select_installation");
                KeyValuePair<string, string> install;
                while (true)
                {
                    var installList = installs.ToList();
                    for (int i = 0; i < installList.Count; i++)
                        Console.WriteLine($"{i + 1}. {installList[i].Key} - {installList[i].Value}");

                    Console.WriteLine();
                    Console.Write("> ");

                    if (int.TryParse(Console.ReadLine(), out int choice) &&
                        choice >= 1 && choice <= installList.Count)
                    {
                        install = installList[choice - 1];
                        break;
                    }

                    translator.print("options.uninstall.invalid_choice");
                }

                // Start
                string smapiPath = Path.Combine(install.Value, "StardewModdingAPI.exe");
                if (!File.Exists(smapiPath))
                {
                    translator.print("start_types.run.smapi_not_found");
                    Console.WriteLine();
                    return;
                }

                var replacements = new Dictionary<string, string>
            {
                { "installName", install.Key },
                { "installDir", install.Value }
            };
                translator.printFormatted("start_types.run.start_run", replacements);
                await new runGame().run(smapiPath);
                Console.WriteLine("\n--------------------------\n");
            }
        }
    }
}
