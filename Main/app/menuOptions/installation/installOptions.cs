using SmapIt.Core;
using SmapIt.Utils;

namespace SmapIt.app.menuOptions.installation
{
    internal class InstallOptions
    {
        public void installOptions()
        {
            Translator translator = new Translator();
            SettingsManager settingsManager = new SettingsManager();

            // Select installation
            var installs = (settingsManager.Settings["_smapitInstalls"] as Newtonsoft.Json.Linq.JObject)?
                .ToObject<Dictionary<string, string>>() ?? new();

            if (installs.Count == 0)
            {
                translator.print("options.uninstall.no_installations");
                return;
            }

            KeyValuePair<string, string> selectedInstall;
            while (true)
            {
                translator.print("options.uninstall.select_installation");

                var installList = installs.ToList();
                for (int i = 0; i < installList.Count; i++)
                    Console.WriteLine($"{i + 1}. {installList[i].Key} - {installList[i].Value}");

                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int choice) &&
                    choice >= 1 && choice <= installList.Count)
                {
                    selectedInstall = installList[choice - 1];
                    break;
                }
                else if (String.IsNullOrEmpty(input))
                {
                    return;
                }

                translator.print("options.uninstall.invalid_choice");
                Console.WriteLine();
            }

            string shortcutPath = Path.Combine(selectedInstall.Value, "SmapIt.lnk");
            var replacements = new Dictionary<string, string>
            {
                { "smapitPath", shortcutPath }
            };

            translator.printFormatted("options.launch_options", replacements);
        }
    }
}
