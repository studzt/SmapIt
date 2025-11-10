using SmapIt.Core;
using SmapIt.Utils;

namespace SmapIt.App.menuOptions
{
    internal class Uninstall
    {
        private const string LOG_IDENT = "App::Uninstall";
        public void uninstall()
        {
            // Classes
            var translator = new Translator();
            var SettingsManager = new SettingsManager();

            var installs = (SettingsManager.Settings["_smapitInstalls"] as Newtonsoft.Json.Linq.JObject)?
                .ToObject<Dictionary<string, string>>() ?? new();

            if (installs.Count == 0)
            {
                translator.print("options.uninstall.no_installations");
                return;
            }

            translator.print("options.uninstall.select_installation");

            var installList = installs.ToList();
            for (int i = 0; i < installList.Count; i++)
                Console.WriteLine($"{i + 1}. {installList[i].Key} - {installList[i].Value}");

            KeyValuePair<string, string> selectedInstall;
            while (true)
            {
                Console.WriteLine();
                Console.Write("> ");
                if (int.TryParse(Console.ReadLine(), out int choice) &&
                    choice >= 1 && choice <= installList.Count)
                {
                    selectedInstall = installList[choice - 1];
                    break;
                }

                translator.print("options.uninstall.invalid_choice");
            }

            var replacements = new Dictionary<string, string>
            {
                { "installName", selectedInstall.Key },
                { "installDir", selectedInstall.Value }
            };
            translator.printFormatted("options.uninstall.confirm", replacements);

            while (true)
            {
                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    translator.print("options.uninstall.invalid_choice");
                    continue;
                }

                switch (input)
                {
                    case "1":
                        AppCore.Logger.WriteLine(LOG_IDENT, $"Uninstalling SmapIt from: {selectedInstall.Value}");
                        translator.print("options.uninstall.uninstalling");

                        try
                        {
                            string shortcutPath = Path.Combine(selectedInstall.Value, "SmapIt.lnk");
                            if (File.Exists(shortcutPath))
                                File.Delete(shortcutPath);

                            installs.Remove(selectedInstall.Key);
                            SettingsManager.Settings["_smapitInstalls"] = installs;
                            SettingsManager.SaveSettings();

                            AppCore.Logger.WriteLine(LOG_IDENT, "Uninstalled successfully.");
                            translator.print("options.uninstall.success");
                            Console.WriteLine();
                            return;
                        }
                        catch (Exception ex)
                        {
                            SentrySdk.CaptureException(ex);
                            AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error when trying to uninstall SmapIt:");
                            AppCore.Logger.WriteException(LOG_IDENT, ex);
                        }

                        return;
                    case "2":
                        translator.print("options.uninstall.cancelled");
                        Console.WriteLine();
                        return;

                    default:
                        translator.print("options.uninstall.invalid_choice");
                        Console.WriteLine();
                        continue;
                }
            }
        }
    }
}
