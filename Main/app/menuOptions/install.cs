using System.Reflection;
using System.Drawing;
using SmapIt.Utils;
using SmapIt.Core;

namespace SmapIt.App.menuOptions
{
    internal class Install
    {
        private const string LOG_IDENT = "App::Install";
        public async Task install()
        {
            var translator = new Translator();

            var SettingsManager = new SettingsManager();
            var installs = (SettingsManager.Settings["_smapitInstalls"] as Newtonsoft.Json.Linq.JObject)?.ToObject<Dictionary<string, string>>() ?? new();

            // Check for installations
            translator.print("options.install.searching_stardew");
            string stardewDirectorySteam = @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley";

            string stardewDirectory = "";
            bool saveInstallation = true;

            // Base files for verification
            bool steamDirValid =
                Directory.Exists(stardewDirectorySteam)
                && System.IO.File.Exists(Path.Combine(stardewDirectorySteam, "Stardew Valley.dll"))
                && System.IO.File.Exists(Path.Combine(stardewDirectorySteam, "Stardew Valley.exe"))
                && System.IO.File.Exists(Path.Combine(stardewDirectorySteam, "Stardew Valley.deps.json"));

            bool hasShortcut = System.IO.File.Exists(Path.Combine(stardewDirectorySteam, "SmapIt.lnk"));
            bool isAlreadyRegistered = installs.Values.Any(dir => string.Equals(Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(stardewDirectorySteam).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));

            // Steam directory is not valid (choose custom directory)
            if (!steamDirValid)
            {
                var result = chooseDirectory();
                stardewDirectory = result.stardewDirectory;
                saveInstallation = result.saveInstallation;
            }

            // Steam directory already have SmapIt installed and registred
            else if (steamDirValid && hasShortcut && isAlreadyRegistered)
            {
                var result = chooseDirectory();
                stardewDirectory = result.stardewDirectory;
                saveInstallation = result.saveInstallation;
            }

            // Steam directory is valid and SmapIt is not installed
            else if (steamDirValid && !hasShortcut && !isAlreadyRegistered)
            {
                translator.print("options.install.found_steam");
                Console.Write("> ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        stardewDirectory = stardewDirectorySteam;
                        break;

                    case "2":
                        var result = chooseDirectory();
                        stardewDirectory = result.stardewDirectory;
                        saveInstallation = result.saveInstallation;
                        break;

                    case "3":
                        return;
                }
            }

            // Steam directory have SmapIt installed but not registred
            else if (steamDirValid && hasShortcut && !isAlreadyRegistered)
            {
                translator.print("options.install.smapit_selectnamesteam");
                while (true)
                {
                    Console.WriteLine("");
                    Console.Write("> ");
                    string? input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        translator.print("options.install.smapit_invalidname");
                        Console.WriteLine("");
                        continue;
                    }

                    if (installs.ContainsKey(input))
                    {
                        translator.print("options.install.smapit_nametaken");
                        continue;
                    }

                    // Save instalation
                    installs[input] = stardewDirectorySteam;
                    SettingsManager.Settings["_smapitInstalls"] = installs;
                    SettingsManager.SaveSettings();

                    break;
                }
            }

            // Steam directory have SmapIt registred but not installed
            else if (steamDirValid && !hasShortcut && isAlreadyRegistered)
            {
                translator.print("options.install.fix_directory");
                Console.WriteLine("");
                Console.Write("> ");
                string? input2 = Console.ReadLine();

                switch (input2)
                {
                    case "1":
                        stardewDirectory = stardewDirectorySteam;
                        saveInstallation = false;
                        break;

                    case "2":
                        break;

                    default:

                        translator.print("options.install.invalid_option");
                        break;
                }
            }

            // == Installing ==
            if (string.IsNullOrWhiteSpace(stardewDirectory))
            {
                return;
            }

            // Installing SMAPI
            AppCore.Logger.WriteLine(LOG_IDENT, $"Starting installation in: {stardewDirectory}");

            translator.print("options.install.smapi_check");
            if (!System.IO.File.Exists(Path.Combine(stardewDirectory, "StardewModdingAPI.exe")))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Starting SMAPI manager");
                translator.print("options.install.smapi_download");
                var SmapiManager = new SmapiUpdater(stardewDirectory);

                bool success = await SmapiManager.InstallSmapi();
                if (!success)
                {
                    translator.print("options.install.smapi_error");
                    return;
                }

                else
                {
                    AppCore.Logger.WriteLine(LOG_IDENT, "SMAPI installation completed successfully.");
                    translator.print("options.install.smapi_success");
                }
            }

            // Installing SmapIt
            Console.WriteLine("");
            AppCore.Logger.WriteLine(LOG_IDENT, "Starting SmapIt installation...");
            translator.print("options.install.smapit_install");

            string shortcutPath = Path.Combine(stardewDirectory, "SmapIt.lnk");
            string exeDirectory = AppContext.BaseDirectory;

            if (string.IsNullOrEmpty(exeDirectory))
            {
                translator.print("options.install.smapit_error");
                AppCore.Logger.WriteLine(LOG_IDENT, "Failed to get exe directory. Try reinstalling SmapIt Manager into a folder.");
                return;
            }

            string exePath = Assembly.GetExecutingAssembly().Location;
            string arguments = $"--start --path \"{Path.Combine(stardewDirectory, "StardewModdingAPI.exe")}\"";

            string iconPath = Path.Combine(exeDirectory, "SmapIt_icon.ico");
            if (!File.Exists(iconPath)){
                Icon? icon = Icon.ExtractAssociatedIcon(exePath);
                if (icon != null)
                {
                    using (FileStream fs = new FileStream(iconPath, FileMode.Create))
                    {
                        icon.Save(fs);
                    }
                }
            }

            Shortcut.Create(exePath, arguments, shortcutPath, iconPath);

            AppCore.Logger.WriteLine(LOG_IDENT, "SmapIt installation completed successfully.");
            translator.print("options.install.smapit_success");

            Console.WriteLine("");

            var replacements = new Dictionary<string, string>
            {
                { "smapitPath", shortcutPath }
            };

            translator.printFormatted("options.install.smapit_path", replacements);
            Console.WriteLine();


            // Saving installation
            if (saveInstallation)
            {
                translator.print("options.install.smapit_selectname");

                while (true)
                {
                    Console.WriteLine("");
                    Console.Write("> ");
                    string? input = Console.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        translator.print("options.install.smapit_invalidname");
                        Console.WriteLine("");
                        continue;
                    }

                    else if (installs.ContainsKey(input))
                    {
                        translator.print("options.install.smapit_nametaken");
                        continue;
                    }

                    else
                    {
                        var smapitInstalls = SettingsManager.Settings["_smapitInstalls"] as Newtonsoft.Json.Linq.JObject;
                        var installsDict = smapitInstalls?.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>();

                        installsDict[input] = stardewDirectory;

                        SettingsManager.Settings["_smapitInstalls"] = installsDict;
                        SettingsManager.SaveSettings();
                        break;
                    }
                }
            }

            Console.WriteLine("");
        }

        private (string stardewDirectory, bool saveInstallation) chooseDirectory()
        {
            var translator = new Translator();
            translator.print("options.install.select_directory");

            var SettingsManager = new SettingsManager();
            var installs = (SettingsManager.Settings["_smapitInstalls"] as Newtonsoft.Json.Linq.JObject)?.ToObject<Dictionary<string, string>>() ?? new();

            while (true)
            {
                Console.WriteLine("");
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input) || !Directory.Exists(input))
                {
                    translator.print("options.install.invalid_directory");
                    continue;
                }

                // Checks if is a valid Stardew valley installation
                bool isValidStardewInstall =
                    System.IO.File.Exists(Path.Combine(input, "Stardew Valley.dll")) &&
                    System.IO.File.Exists(Path.Combine(input, "Stardew Valley.exe")) &&
                    System.IO.File.Exists(Path.Combine(input, "Stardew Valley.deps.json"));

                if (!isValidStardewInstall)
                {
                    translator.print("options.install.invalid_directory");
                    continue;
                }

                bool alreadyHasShortcut = System.IO.File.Exists(Path.Combine(input, "SmapIt.lnk"));
                bool alreadyRegistered = installs.Values.Any(dir => string.Equals(dir, input, StringComparison.OrdinalIgnoreCase));

                if (alreadyHasShortcut && alreadyRegistered)
                {
                    translator.print("options.install.invalid_directory");
                    continue;
                }

                else if (!alreadyHasShortcut && !alreadyRegistered)
                {
                    return (input, true);
                }

                else if (alreadyHasShortcut && !alreadyRegistered)
                {
                    translator.print("options.install.smapit_selectname");
                    while (true)
                    {
                        Console.WriteLine("");
                        Console.Write("> ");
                        string? input2 = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input2))
                        {
                            translator.print("options.install.smapit_invalidname");
                            Console.WriteLine("");
                            continue;
                        }

                        if (installs.ContainsKey(input2))
                        {
                            translator.print("options.install.smapit_nametaken");
                            continue;
                        }

                        // Save instalation
                        installs[input2] = input;
                        SettingsManager.Settings["_smapitInstalls"] = installs;
                        SettingsManager.SaveSettings();

                        return ("", false);
                    }
                }

                else if (!alreadyHasShortcut && alreadyRegistered)
                {
                    translator.print("options.install.fix_directory");
                    Console.WriteLine("");
                    Console.Write("> ");
                    string? input2 = Console.ReadLine();

                    switch (input2)
                    {
                        case "1":
                            return (input, false);
                        case "2":
                            continue;
                        default:
                            translator.print("options.install.invalid_option");
                            continue;
                    }
                }
            }
        }

    }
}