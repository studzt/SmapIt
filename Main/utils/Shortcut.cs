using WindowsShortcutFactory;
using SmapIt.Core;

namespace SmapIt.Utils
{
    internal static class Shortcut
    {
        private const string LOG_IDENT = "Utils::Shortcut";

        public static void Create(string exePath, string exeArgs, string lnkPath, string iconPath)
        {
            if (File.Exists(lnkPath))
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Shortcut already exists: {lnkPath}");
                return;
            }

            try
            {
                using var shortcut = new WindowsShortcut
                {
                    Path = exePath,
                    Arguments = exeArgs,
                    IconLocation = iconPath
                };

                shortcut.Save(lnkPath);

                AppCore.Logger.WriteLine(LOG_IDENT, $"Shortcut created successfully: {lnkPath}");
            }
            catch (FileNotFoundException ex)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Failed to create shortcut for {lnkPath}");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
            }
            catch (Exception ex)
            {
                AppCore.Logger.WriteLine(LOG_IDENT, $"Unexpected error while creating shortcut");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}