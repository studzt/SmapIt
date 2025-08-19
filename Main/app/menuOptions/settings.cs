using main.classes;

namespace main.app.menuOptions
{
    internal class Settings
    {
        public void settings()
        {
            // Classes
            settingsManager settingsManager = new settingsManager();
            Translator translator = new Translator();

            // Utils
            string[] languages = translator.getLanguageList();

            // Settings
            void changeLanguage()
            {
                translator.print("options.settings.choose_lang");

                for (int i = 0; i < languages.Length; i++)
                {
                    Console.WriteLine($"{i + 1}) {languages[i]}");
                }

                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int choice))
                {
                    if (choice >= 1 && choice <= languages.Length)
                    {
                        string selectedLanguage = languages[choice - 1];
                        settingsManager.Settings["lang"] = selectedLanguage;
                        settingsManager.SaveSettings();

                        translator.print("options.settings.success");
                        translator.print("options.settings.restart");
                    }
                    else
                    {
                        translator.print("options.settings.invalid_choice");
                    }
                }
                else
                {
                    translator.print("options.settings.invalid_choice");
                }
            }

            void tglConsoleSmapi()
            {
                translator.print("options.settings.confirm");
                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (input == "1")
                {
                    bool current = Convert.ToBoolean(settingsManager.Settings["hide_smapi_console"]);
                    settingsManager.Settings["hide_smapi_console"] = !current;
                    settingsManager.SaveSettings();

                    translator.print("options.settings.success");
                    translator.print("options.settings.restart");
                }
            }

            void tglUpdSmapi()
            {
                translator.print("options.settings.confirm");
                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (input == "1")
                {
                    bool current = Convert.ToBoolean(settingsManager.Settings["auto_update_smapi"]);
                    settingsManager.Settings["auto_update_smapi"] = !current;
                    settingsManager.SaveSettings();

                    translator.print("options.settings.success");
                    translator.print("options.settings.restart");
                }
            }

            void tglUpdSmapit()
            {
                translator.print("options.settings.confirm");
                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (input == "1")
                {
                    bool current = Convert.ToBoolean(settingsManager.Settings["auto_update_smapit"]);
                    settingsManager.Settings["auto_update_smapit"] = !current;
                    settingsManager.SaveSettings();

                    translator.print("options.settings.success");
                    translator.print("options.settings.restart");
                }
            }

            // Main
            while (true)
            {
                // Current settings
                string currentLang = settingsManager.Settings["lang"] as string ?? "";
                bool hideConsole = Convert.ToBoolean(settingsManager.Settings["hide_smapi_console"]);
                bool autoUpdateSmapi = Convert.ToBoolean(settingsManager.Settings["auto_update_smapi"]);
                bool autoUpdateSmapIt = Convert.ToBoolean(settingsManager.Settings["auto_update_smapit"]);

                var replacements = new Dictionary<string, string>
                    {
                        { "lang", "[yellow]" + currentLang },
                        { "hSmapiConsole", hideConsole ? translator.translateSingle("options.settings.true") : translator.translateSingle("options.settings.false") },
                        { "aUpdateSmapi", autoUpdateSmapi ? translator.translateSingle("options.settings.true") : translator.translateSingle("options.settings.false") },
                        { "aUpdateSmapIt", autoUpdateSmapIt ? translator.translateSingle("options.settings.true") : translator.translateSingle("options.settings.false") }
                    };

                Console.WriteLine();
                translator.printFormatted("options.settings.settings", replacements);

                Console.Write("\n> ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        changeLanguage();
                        break;
                    case "2":
                        tglConsoleSmapi();
                        break;

                    case "3":
                        tglUpdSmapi();
                        break;

                    case "4":
                        tglUpdSmapit();
                        break;

                    case "5":
                        return;

                    default:
                        translator.print("options.settings.invalid_choice");
                        break;
                }
            }



        }
    }
}