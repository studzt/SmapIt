using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace main.classes
{
    public class Translator
    {
        // Utils
        private JObject translations;

        // Constructor
        public Translator(string language = "")
        {
            if (language == "")
            {
                language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }

            translations = new JObject();
            loadTranslations(language);
        }

        // Load translations from JSON files
        private void loadTranslations(string language)
        {
            try
            {
                string[] validLanguages = getLanguageList();
                if (!validLanguages.Contains(language))
                {
                    Console.WriteLine("Invalid language. Defaulting to English.");
                    language = "en";
                }

                string resourceName = $"Updater.lang.{language}.json";
                using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Console.WriteLine("Translation resource not found.");
                    translations = new JObject();
                    return;
                }

                using StreamReader reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                translations = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load translations: {ex.Message}");
                translations = new JObject();
            }
        }
        public string[] getLanguageList()
        {
            var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            return resourceNames
                .Where(name => name.StartsWith("Updater.lang.") && name.EndsWith(".json"))
                .Select(name => name.Split('.')[2])
                .ToArray();
        }

        // Translate key before printing
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

        // Main function
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
    }
}
