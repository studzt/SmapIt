using Newtonsoft.Json;
using System.Reflection;

namespace main.classes
{
    internal class settingsManager
    {
#if (DEBUG)
        private readonly string settingsPath = Path.Combine("..", "..", "..", "settings.json");
#else
        private readonly string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
#endif

        // Default values
        private readonly Dictionary<string, object> defaultSettings = new()
        {
            { "lang", "" },
            { "_smapitInstalls", new Dictionary<string, string>() },
            { "hide_smapi_console", "true" },
            { "auto_update_smapi", "true" },
            { "auto_update_smapit", "true" }
        };

        public Dictionary<string, object> Settings { get; private set; }

        public settingsManager()
        {
            Settings = new Dictionary<string, object>();
            LoadOrInitializeSettings();
        }

        private void LoadOrInitializeSettings()
        {
            if (!File.Exists(settingsPath))
            {
                Settings = new Dictionary<string, object>(defaultSettings);
                SaveSettings();
                return;
            }

            var content = File.ReadAllText(settingsPath);
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
