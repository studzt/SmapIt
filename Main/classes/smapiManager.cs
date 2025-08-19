using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace main.classes
{
    internal class smapiManager
    {
        // Constructor
        private readonly string stardewDirectory;
        public smapiManager(string stardewDirectory)
        {
            this.stardewDirectory = stardewDirectory;
        }

        // Main function
        public async Task<bool> installSmapi()
        {
            var translator = new Translator();

            if (!(Directory.Exists(stardewDirectory)
                && System.IO.File.Exists(Path.Combine(stardewDirectory, "Stardew Valley.dll"))
                && System.IO.File.Exists(Path.Combine(stardewDirectory, "Stardew Valley.exe"))
                && System.IO.File.Exists(Path.Combine(stardewDirectory, "Stardew Valley.deps.json"))))
            {
                return false;
            }

            try
            {
                // Checks internet connection
                bool internetAvaiable = await checkInternetConnection();

                if (!internetAvaiable)
                {
                    translator.print("options.install.github_not_available");
                    return false;
                }

                // Create temp folder
                string tempPath = Path.Combine(Path.GetTempPath(), "SmapIt");
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);

                // Download smapi
                string smapiApiUrl = "https://api.github.com/repos/Pathoschild/SMAPI/releases/latest";
                string zipFilePath = Path.Combine(tempPath, "SMAPI.zip");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "SmapIt");

                    HttpResponseMessage response = await client.GetAsync(smapiApiUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    var releaseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                    var assets = releaseData?["assets"] as Newtonsoft.Json.Linq.JArray;

                    if (assets == null)
                        return false;

                    // Searching .zip file in github
                    string? downloadUrl = assets
                        .OfType<Newtonsoft.Json.Linq.JObject>()
                        .Select(asset =>
                        {
                            var name = asset["name"]?.ToString();
                            var url = asset["browser_download_url"]?.ToString();

                            if (!string.IsNullOrEmpty(name)
                                && name.Contains("SMAPI")
                                && !name.Contains("for-developers")
                                && !string.IsNullOrEmpty(url))
                            {
                                return url;
                            }

                            return null;
                        })
                        .FirstOrDefault(url => url != null);

                    if (string.IsNullOrEmpty(downloadUrl))
                        return false;

                    // Download zip file
                    using (Stream streamToReadFrom = await client.GetStreamAsync(downloadUrl))
                    using (Stream streamToWriteTo = File.Open(zipFilePath, FileMode.Create))
                    {
                        streamToReadFrom.CopyTo(streamToWriteTo);
                    }

                }


                translator.print("options.install.smapi_install");

                // Decompile zip file
                string extractPath = Path.Combine(tempPath, "SMAPI");
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);

                // Getting internal/windows folder
                string? internalWindowsPath = Directory
                    .GetDirectories(extractPath)
                    .Select(subfolder =>
                    {
                        string path = Path.Combine(subfolder, "internal", "windows");
                        return Directory.Exists(path) ? path : null;
                    })
                    .FirstOrDefault(path => path != null);


                if (internalWindowsPath == null)
                    return false;

                // Start the installer
                string arguments = $"--no-prompt --install --game-path \"{stardewDirectory}\"";
                string installerPath = Path.Combine(internalWindowsPath, "SMAPI.Installer.exe");

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                if (process != null)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                // Delete temp folder
                string tempPath = Path.Combine(Path.GetTempPath(), "SmapIt");
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }

            return true;
        }

        private async Task<bool> checkInternetConnection()
        {
            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                string URL = "https://github.com";
                HttpResponseMessage response = await httpClient.GetAsync(URL);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
