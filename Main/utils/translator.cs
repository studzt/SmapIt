using Newtonsoft.Json.Linq;
using SmapIt.Core;
using System.Diagnostics;
using System.Globalization;

namespace SmapIt.Utils
{
    public class Translator
    {
        private const string LOG_IDENT = "Utils::Translator";

        private JObject translations;
        private static string langFolderPath = "";
        private static string defaultLangFile = Debugger.IsAttached ? "pt.json" : "en.json";

        // Constructor
        public Translator(string language = "")
        {
            translations = new JObject();
            langFolderPath = Path.Combine(AppContext.BaseDirectory, "lang");

            if (!Directory.Exists(langFolderPath))
            {
                Console.WriteLine("Failed to load Translator.cs. Check logs for more information.");
                AppCore.Logger.WriteLine(LOG_IDENT, $"lang folder not found in: {langFolderPath}");

                Console.ReadLine();
                Environment.Exit(1);
            }

            if (!File.Exists(Path.Combine(langFolderPath, defaultLangFile)))
            {
                Console.WriteLine("Failed to load Translator.cs. Check logs for more information.");
                AppCore.Logger.WriteLine(LOG_IDENT, $"en.json not found in: {Path.Combine(langFolderPath, "en.json")}");

                Console.ReadLine();
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(language))
                language = getLanguage();

            loadTranslations(language);
        }

        // == Core Functions ==
        public string getLanguage()
        {
            var SettingsManager = new SettingsManager();
            string language;

            if (!SettingsManager.Settings.TryGetValue("lang", out var langObj) || string.IsNullOrEmpty(langObj as string))
            {
                language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                SettingsManager.Settings["lang"] = language;
                SettingsManager.SaveSettings();
            }
            else
            {
                language = (SettingsManager.Settings["lang"] as string) ?? "en";
            }

            string[] validLanguages = getLanguageList();
            if (!validLanguages.Contains(language))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Language not found: {language}. Defaulting to English.");
                language = "en";

                SettingsManager.Settings["lang"] = language;
                SettingsManager.SaveSettings();
            }

            return language;
        }

        private void loadTranslations(string language)
        {
            try
            {
                string filePath = Path.Combine(langFolderPath, $"{language}.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    translations = JObject.Parse(json);
                }
                else
                {
                    Console.WriteLine($"Translation file not found: {filePath}");
                    translations = new JObject();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load translations: {ex.Message}");
                translations = new JObject();
            }
        }

        private List<string> translate(string key)
        {
            JToken? current = translations;

            foreach (var part in key.Split('.'))
            {
                if (current == null)
                    return new List<string> { $"[red]Translation key not found: {key}" };

                current = current[part];
            }

            if (current is JArray array)
                return array.Select(t => t.ToString()).ToList();

            if (current is JValue val)
                return new List<string> { val.ToString() };

            return new List<string> { $"[red]Invalid format for key: {key}" };
        }

        public string[] getLanguageList()
        {
            return Directory.GetFiles(langFolderPath, "*.json")
                            .Select(file => Path.GetFileNameWithoutExtension(file))
                            .ToArray();
        }

        // == Main Functions ==
        public string translateSingle(string key)
        {
            List<string> translatedLines = translate(key);
            return translatedLines.FirstOrDefault() ?? $"[red]Translation key not found: {key}";
        }

        public void print(string key)
        {
            List<string> translatedLines = translate(key);
            ConsoleColor originalColor = Console.ForegroundColor;
            ConsoleColor currentColor = originalColor;

            foreach (var line in translatedLines)
            {
                int i = 0;
                while (i < line.Length)
                {
                    if (line[i] == '[')
                    {
                        int endIndex = line.IndexOf(']', i);
                        if (endIndex != -1)
                        {
                            string colorName = line.Substring(i + 1, endIndex - i - 1);
                            if (Enum.TryParse(colorName, true, out ConsoleColor color))
                            {
                                currentColor = color;
                                Console.ForegroundColor = currentColor;
                                i = endIndex + 1;
                                continue;
                            }
                        }
                    }

                    Console.Write(line[i]);
                    i++;
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = originalColor;
        }

        public void printFormatted(string key, Dictionary<string, string> replacements)
        {
            List<string> translatedLines = translate(key);
            ConsoleColor originalColor = Console.ForegroundColor;
            ConsoleColor currentColor = originalColor;

            foreach (var line in translatedLines)
            {
                string processedLine = line;

                foreach (var replacement in replacements)
                {
                    processedLine = processedLine.Replace($"{{{replacement.Key}}}", replacement.Value);
                }

                int i = 0;
                while (i < processedLine.Length)
                {
                    if (processedLine[i] == '[')
                    {
                        int endIndex = processedLine.IndexOf(']', i);
                        if (endIndex != -1)
                        {
                            string colorName = processedLine.Substring(i + 1, endIndex - i - 1);
                            if (Enum.TryParse(colorName, true, out ConsoleColor color))
                            {
                                currentColor = color;
                                Console.ForegroundColor = currentColor;
                                i = endIndex + 1;
                                continue;
                            }
                        }
                    }

                    Console.Write(processedLine[i]);
                    i++;
                }
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}