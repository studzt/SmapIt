using System.Reflection;
using SmapIt.Core;

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

                try
                {
                    AppCore.Logger.WriteLine(LOG_IDENT, $"Moving default mods to: {profilePath}");
                    string defaultModsPrefix = "SmapIt.resources.defaultMods.";
                    var defaultMods = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                        .Where(name => name.StartsWith(defaultModsPrefix))
                        .ToList();

                    foreach (string mod in defaultMods)
                    {
                        AppCore.Logger.WriteLine(LOG_IDENT, $"Moving: {mod}");

                        string relative = mod.Substring(defaultModsPrefix.Length);
                        relative = relative.Replace('.', Path.DirectorySeparatorChar);

                        int lastSep = relative.LastIndexOf(Path.DirectorySeparatorChar);
                        if (lastSep >= 0)
                        {
                            relative = relative[..lastSep] + "." + relative[(lastSep + 1)..];
                        }

                        string modFolder = Path.Combine(profilePath, Path.GetDirectoryName(relative)!);
                        if (!Directory.Exists(modFolder))
                        {
                            Directory.CreateDirectory(modFolder);
                        }

                        Stream? file = Assembly.GetExecutingAssembly().GetManifestResourceStream(mod);
                        using FileStream output = new(Path.Combine(profilePath, relative), FileMode.Create, FileAccess.ReadWrite);
                        file!.CopyTo(output);
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    AppCore.Logger.WriteLine(LOG_IDENT, $"Failed to move default mods to: {profilePath}");
                    AppCore.Logger.WriteException(LOG_IDENT, ex);
                }

                return profilePath;
            }

            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                translator.print("options.profiles.error");
                AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error when trying to create a new profile:");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }

        public static bool Delete(string profile)
        {
            var SettingsManager = new SettingsManager();
            var translator = new Translator();

            try
            {
                string[] profileList = profileManager.GetProfiles();
                if (profileList.Length <= 1)
                {
                    AppCore.Logger.WriteLine(LOG_IDENT, "Attempt to delete the last profile: setting default profile to none.");
                    SettingsManager.Settings["default_profile"] = "none";
                    SettingsManager.SaveSettings();
                    translator.print("options.profiles.profile_set_default");
                }

                if (profile == (string)SettingsManager.Settings["default_profile"])
                {
                    AppCore.Logger.WriteLine(LOG_IDENT, "Attempt to delete the profile that was set to default: setting default profile to none.");
                    SettingsManager.Settings["default_profile"] = "none";
                    SettingsManager.SaveSettings();
                    translator.print("options.profiles.profile_set_default");
                }

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

            catch (IOException ex) when ((uint)ex.HResult == 0x80070020)
            {
                SentrySdk.CaptureException(ex);
                translator.print("options.profiles.del_file_being_used");
                AppCore.Logger.WriteLine(LOG_IDENT, "File is being used by another process.");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                return false;
            }

            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
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
                SentrySdk.CaptureException(ex);
                translator.print("options.profiles.error");
                AppCore.Logger.WriteLine(LOG_IDENT, "Unexpected error when trying to get profile:");
                AppCore.Logger.WriteException(LOG_IDENT, ex);
                return null;
            }
        }
    }
}