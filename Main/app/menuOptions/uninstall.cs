using main.classes;

namespace main.app.menuOptions
{
    internal class Uninstall
    {
        public void uninstall()
        {
            // Classes
            var translator = new Translator();
            var settingsManager = new settingsManager();

            // Get saved installations
            var installs = (settingsManager.Settings["_smapitInstalls"] as Newtonsoft.Json.Linq.JObject)?
                .ToObject<Dictionary<string, string>>() ?? new();

            if (installs.Count == 0)
            {
                translator.print("options.uninstall.no_installations");
                return;
            }

            // Display available installations
            translator.print("options.uninstall.select_installation");

            var installList = installs.ToList();
            for (int i = 0; i < installList.Count; i++)
                Console.WriteLine($"{i + 1}. {installList[i].Key} - {installList[i].Value}");

            // Select installation
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

            // Confirmation
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
                    case "1": // Confirm uninstall
                        translator.print("options.uninstall.uninstalling");

                        string shortcutPath = Path.Combine(selectedInstall.Value, "SmapIt.lnk");
                        if (File.Exists(shortcutPath))
                            File.Delete(shortcutPath);

                        installs.Remove(selectedInstall.Key);
                        settingsManager.Settings["_smapitInstalls"] = installs;
                        settingsManager.SaveSettings();

                        translator.print("options.uninstall.success");
                        Console.WriteLine();
                        return;

                    case "2": // Cancel
                        translator.print("options.uninstall.cancelled");
                        Console.WriteLine();
                        return;

                    default: // Invalid input
                        translator.print("options.uninstall.invalid_choice");
                        Console.WriteLine();
                        continue;
                }
            }
        }
    }
}
