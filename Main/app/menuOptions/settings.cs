using SmapIt.Core;
using SmapIt.Utils;

namespace SmapIt.App.menuOptions
{
    internal class Settings
    {
        public void settings()
        {
            // Classes
            var SettingsManager = new SettingsManager();
            var translator = new Translator();

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

                Console.Write("\n> ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int choice))
                {
                    if (choice >= 1 && choice <= languages.Length)
                    {
                        string selectedLanguage = languages[choice - 1];
                        SettingsManager.Settings["lang"] = selectedLanguage;
                        SettingsManager.SaveSettings();

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
                    bool current = Convert.ToBoolean(SettingsManager.Settings["hide_smapi_console"]);
                    SettingsManager.Settings["hide_smapi_console"] = !current;
                    SettingsManager.SaveSettings();

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
                    bool current = Convert.ToBoolean(SettingsManager.Settings["auto_update_smapi"]);
                    SettingsManager.Settings["auto_update_smapi"] = !current;
                    SettingsManager.SaveSettings();

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
                    bool current = Convert.ToBoolean(SettingsManager.Settings["auto_update_smapit"]);
                    SettingsManager.Settings["auto_update_smapit"] = !current;
                    SettingsManager.SaveSettings();

                    translator.print("options.settings.success");
                    translator.print("options.settings.restart");
                }
            }

            void changeDefaultProfile()
            {
                string[] profileList = profileManager.GetProfiles();
                string? selectedProfile;

                if (profileList.Length <= 0)
                {
                    translator.print("options.profiles.any_profile");
                    return;
                }

                translator.print("options.profiles.choose_profile");
                for (int i = 0; i < profileList.Length; i++)
                {
                    Console.WriteLine($"{i + 1}) {profileList[i]}");
                }
                Console.WriteLine($"{profileList.Length + 1}) {translator.translateSingle("options.settings.any")}");

                Console.Write("\n> ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int choice))
                {
                    if (choice >= 1 && choice <= profileList.Length)
                    {
                        selectedProfile = profileList[choice - 1];
                        SettingsManager.Settings["default_profile"] = selectedProfile;
                        SettingsManager.SaveSettings();
                        translator.print("options.settings.success");
                    }
                    else if (choice == profileList.Length + 1)
                    {
                        selectedProfile = "none";
                        SettingsManager.Settings["default_profile"] = selectedProfile;
                        SettingsManager.SaveSettings();
                        translator.print("options.settings.success");
                    }
                    else
                    {
                        translator.print("options.profiles.invalid_choice");
                    }
                }
                else
                {
                    translator.print("options.profiles.invalid_choice");
                    return;
                }
            }

            void tglAskProfile()
            {
                translator.print("options.settings.confirm");
                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (input == "1")
                {
                    bool current = Convert.ToBoolean(SettingsManager.Settings["always_ask_profile"]);
                    SettingsManager.Settings["always_ask_profile"] = !current;
                    SettingsManager.SaveSettings();

                    translator.print("options.settings.success");
                }
            }

            // Main
            while (true)
            {
                // Current settings
                string currentLang = SettingsManager.Settings["lang"] as string ?? "";
                bool hideConsole = Convert.ToBoolean(SettingsManager.Settings["hide_smapi_console"]);
                bool autoUpdateSmapi = Convert.ToBoolean(SettingsManager.Settings["auto_update_smapi"]);
                bool autoUpdateSmapIt = Convert.ToBoolean(SettingsManager.Settings["auto_update_smapit"]);
                string defaultProfile = SettingsManager.Settings["default_profile"] as string ?? "none";
                bool askProfile = Convert.ToBoolean(SettingsManager.Settings["always_ask_profile"]);

                var replacements = new Dictionary<string, string>
                {
                    { "lang", currentLang },
                    { "hSmapiConsole", hideConsole ? translator.translateSingle("options.settings.true") : translator.translateSingle("options.settings.false") },
                    { "aUpdateSmapi", autoUpdateSmapi ? translator.translateSingle("options.settings.true") : translator.translateSingle("options.settings.false") },
                    { "aUpdateSmapIt", autoUpdateSmapIt ? translator.translateSingle("options.settings.true") : translator.translateSingle("options.settings.false") },
                    { "defaultProfile", defaultProfile == "none" ? translator.translateSingle("options.settings.any") : defaultProfile },
                    { "askProfile", askProfile ? translator.translateSingle("options.settings.true") : translator.translateSingle("options.settings.false") },
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
                        changeDefaultProfile();
                        break;

                    case "6":
                        tglAskProfile();
                        break;

                    case "7":
                        return;

                    default:
                        translator.print("options.settings.invalid_choice");
                        break;
                }
            }



        }
    }
}