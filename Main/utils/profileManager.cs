using System;
using SmapIt.Core;
using SmapIt.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmapIt.Utils
{
    internal class profileManager
    {
        private const string LOG_IDENT = "Utils::profileManager";
        private static readonly string exeDir = AppContext.BaseDirectory;

        public static string? Create(string name)
        {
            var translator = new Translator();

            try
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Creating a new profile...");

                if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name == "none")
                {
                    translator.print("options.profiles.error");
                    AppCore.Logger.WriteLine(LOG_IDENT, "Invalid characters found in profile name.");
                    return null;
                }

                string profilesPath = Directory.CreateDirectory(Path.Combine(exeDir, "Profiles")).FullName;
                string profilePath = Directory.CreateDirectory(Path.Combine(profilesPath, name)).FullName;

                AppCore.Logger.WriteLine(LOG_IDENT, $"Successfully created new profile in: {profilePath}");
                return profilePath;
            }

            catch (Exception ex)
            {
                translator.print("options.profiles.error");
                AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error when trying to create a new profile:");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }

        public static bool Delete(string profile)
        {
            var translator = new Translator();

            try
            {
                AppCore.Logger.WriteLine(LOG_IDENT, "Deleting profile...");

                string profilesPath = Directory.CreateDirectory(Path.Combine(exeDir, "Profiles")).FullName;
                string profilePath = Path.Combine(profilesPath, profile);

                if (!Directory.Exists(profilePath))
                {
                    translator.print("options.profiles.error");
                    AppCore.Logger.WriteLine(LOG_IDENT, "Profile not found.");
                    return false;
                }

                Directory.Delete(profilePath, true);
                AppCore.Logger.WriteLine(LOG_IDENT, $"Profile deleted successfully in: {profilePath}");
                return true;
            }

            catch (Exception ex)
            {
                translator.print("options.profiles.error");
                AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error when trying to delete a profile:");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                return false;
            }
        }

        public static string[] GetProfiles()
        {
            string profilesPath = Directory.CreateDirectory(Path.Combine(exeDir, "Profiles")).FullName;

            return Directory.GetDirectories(profilesPath)
                .Select(dir => Path.GetFileName(dir))
                .ToArray();
        }

        public static string? GetProfilePath(string profile)
        {
            var translator = new Translator();

            try
            {
                string profilesPath = Directory.CreateDirectory(Path.Combine(exeDir, "Profiles")).FullName;
                string profilePath = Path.Combine(profilesPath, profile);

                if (!Directory.Exists(profilePath))
                {
                    translator.print("options.profiles.error");
                    AppCore.Logger.WriteLine(LOG_IDENT, "Profile not found.");
                    return null;
                }

                return profilePath;
            }
            catch (Exception ex)
            {
                translator.print("options.profiles.error");
                AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error when trying to get profile:");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }
    }
}
