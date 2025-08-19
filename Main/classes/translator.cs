using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace main.classes
{
    public class Translator
    {
        private JObject translations;

        // Constructor
        public Translator(string language = "")
        {
            if (language == "")
            {
                language = getLanguage();
            }

            translations = new JObject();
            loadTranslations(language);
        }

        // == Core Functions ==
        public string getLanguage()
        {
            var settingsManager = new settingsManager();
            string language = settingsManager.Settings.ContainsKey("lang") ? (settingsManager.Settings["lang"] as string) ?? "" : "";

            if (string.IsNullOrEmpty(language))
            {
                language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                settingsManager.Settings["lang"] = language;
                settingsManager.SaveSettings();
            }

            string[] validLanguages = getLanguageList();
            if (!validLanguages.Contains(language))
            {
                Console.WriteLine("Invalid language. Defaulting to English.");
                language = "en";

                settingsManager.Settings["lang"] = language;
                settingsManager.SaveSettings();
            }

            return language;
        }

        private void loadTranslations(string language)
        {
            try
            {
#if DEBUG
                string filePath = Path.Combine("..", "..", "..", "lang", $"{language}.json");
#else
                    string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string filePath = Path.Combine(exeDir, "lang", $"{language}.json");
#endif


                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    translations = JObject.Parse(json);
                }
                else
                {
                    Console.WriteLine("Translation file not found.");
                    Console.WriteLine(filePath);
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
#if DEBUG
            string langFolderPath = Path.Combine("..", "..", "..", "lang");
#else
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string langFolderPath = Path.Combine(exeDir, "lang");
#endif

            if (!Directory.Exists(langFolderPath))
            {
                Console.WriteLine("Language folder not found.");
                return new string[] { };
            }

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