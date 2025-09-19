// https://github.com/bloxstraplabs/bloxstrap/blob/main/Bloxstrap/Utility/Shortcut.cs

using System;
using System.IO;
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
                var shortcut = ShellLink.Shortcut.CreateShortcut(exePath, exeArgs, lnkPath, iconPath, 0);
                shortcut.WriteToFile(lnkPath);

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