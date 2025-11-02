using Newtonsoft.Json;

namespace SmapIt.Core
{
    internal class SettingsManager
    {
        private const string LOG_IDENT = "Utils::SettingsManager";
        private static string settingsPath = "";

        // Default values
        private readonly Dictionary<string, object> defaultSettings = new()
        {
            { "lang", "" },
            { "_smapitInstalls", new Dictionary<string, string>() },
            { "hide_smapi_console", "true" },
            { "auto_update_smapi", "true" },
            { "auto_update_smapit", "true" },
            { "default_profile", "none" },
            { "always_ask_profile", "false" }
        };

        public Dictionary<string, object> Settings { get; private set; }

        public SettingsManager()
        {
            Settings = new Dictionary<string, object>();
            LoadOrInitializeSettings();
        }

        private void LoadOrInitializeSettings()
        {
            string? exeDir = Path.GetDirectoryName(AppContext.BaseDirectory);
            if (!string.IsNullOrEmpty(exeDir))
            {
                settingsPath = Path.Combine(exeDir, "settings.json");
            }
            else
            {
                Console.WriteLine("Unexpected error. Check logs for more information.");
                AppCore.Logger.WriteLine(LOG_IDENT, "Failed to get current directory: Got null or empty. Try reinstalling inside of a folder.");
                return;
            }

            if (!File.Exists(settingsPath))
            {
                Settings = new Dictionary<string, object>(defaultSettings);
                SaveSettings();
                return;
            }

            string content = File.ReadAllText(settingsPath);
            var loaded = JsonConvert.DeserializeObject<Dictionary<string, object>>(content)
                         ?? new Dictionary<string, object>();

            foreach (var pair in defaultSettings)
            {
                if (!loaded.ContainsKey(pair.Key))
                    loaded[pair.Key] = pair.Value;
            }

            Settings = loaded;
            SaveSettings();
        }

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(settingsPath, json);
        }
    }
}
