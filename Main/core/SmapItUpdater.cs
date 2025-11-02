using SmapIt.Utils;
using System.Diagnostics;

namespace SmapIt.Core
{
    internal class SmapItUpdater
    {
        private const string LOG_IDENT = "Core::SmapItUpdater";
        private static readonly string exeDir = AppContext.BaseDirectory;

        public async Task Update(HttpClient client, string smapitShortcut)
        {
            var Mutex = new AsyncMutex("SmapIt_Updater");
            var translator = new Translator();

            try
            {
                await Mutex.AcquireAsync(CancellationToken.None);

                string updaterPath = Path.Combine(exeDir, "SmapIt Updater.exe");
                if (!File.Exists(updaterPath))
                {
                    translator.print("start_types.run.error_smapit");
                    AppCore.Logger.WriteLine(LOG_IDENT, $"SmapIt Updater.exe not found in: {updaterPath}");
                    return;
                }

                string currentProcessId = Process.GetCurrentProcess().Id.ToString();

                string tempDir = Path.Combine(Path.GetTempPath(), "SmapIt");
                prepareTempDir(tempDir);

                string tempUpdaterPath = Path.Combine(tempDir, "Updater.exe");
                File.Copy(updaterPath, tempUpdaterPath, overwrite: true);

                Process.Start(new ProcessStartInfo
                {
                    FileName = tempUpdaterPath,
                    Arguments = $"{currentProcessId} {translator.getLanguage()} \"{smapitShortcut}\"",
                    UseShellExecute = true
                });

                Environment.Exit(0);
            }

            catch (Exception ex)
            {
                translator.print("start_types.run.error_smapit");
                AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error when trying to update SmapIt:");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
            }

            finally
            {
                await Mutex.ReleaseAsync();
            }
        }

        private void prepareTempDir(string path)
        {
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                foreach (var file in dir.GetFiles()) file.Delete();
                foreach (var subDir in dir.GetDirectories()) subDir.Delete(true);
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
