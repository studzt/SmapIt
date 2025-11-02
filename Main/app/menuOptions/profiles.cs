using SmapIt.Utils;
using System.Diagnostics;

namespace SmapIt.app.menuOptions
{
    internal class Profiles
    {
        public void profiles()
        {
            var translator = new Translator();

            string? chooseProfile()
            {
                string[] profileList = profileManager.GetProfiles();

                if (profileList.Length <= 0)
                {
                    translator.print("options.profiles.any_profile");
                    return null;
                }

                translator.print("options.profiles.choose_profile");
                for (int i = 0; i < profileList.Length; i++)
                {
                    Console.WriteLine($"{i + 1}) {profileList[i]}");
                }

                Console.Write("\n> ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int choice))
                {
                    if (choice >= 1 && choice <= profileList.Length)
                    {
                        string selectedProfile = profileList[choice - 1];
                        return selectedProfile;
                    }
                    else
                    {
                        translator.print("options.profiles.invalid_choice");
                    }
                }
                else
                {
                    translator.print("options.profiles.invalid_choice");
                }

                return null;
            }

            while (true)
            {
                Console.WriteLine();
                translator.print("options.profiles.main");
                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        {
                            translator.print("options.profiles.new_name");
                            while (true)
                            {
                                Console.Write("\n> ");
                                string? profileName = Console.ReadLine();

                                if (profileName == null || profileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || profileName == "none")
                                {
                                    translator.print("options.profiles.invalid_name");
                                    continue;
                                }

                                string? profilePath = profileManager.Create(profileName);
                                if (profilePath == null)
                                {
                                    break;
                                }

                                var replacements = new Dictionary<string, string>
                                {
                                    { "profilePath", profilePath }
                                };

                                translator.printFormatted("options.profiles.new_success", replacements);
                                break;
                            }

                            break;
                        }

                    case "2":
                        {
                            string? choosenProfile = chooseProfile();
                            if (String.IsNullOrEmpty(choosenProfile))
                            {
                                break;
                            }

                            string? profilePath = profileManager.GetProfilePath(choosenProfile);
                            if (String.IsNullOrEmpty(profilePath))
                            {
                                break;
                            }

                            try
                            {
                                Process.Start("explorer.exe", profilePath);
                            }
                            catch (Exception)
                            {
                                var replacements = new Dictionary<string, string>
                                {
                                    { "profilePath", profilePath }
                                };

                                translator.printFormatted("options.profiles.profile_path", replacements);
                            }
                            break;
                        }


                    case "3":
                        {
                            string? choosenProfile = chooseProfile();
                            if (String.IsNullOrEmpty(choosenProfile))
                            {
                                break;
                            }

                            string? profilePath = profileManager.GetProfilePath(choosenProfile);
                            if (String.IsNullOrEmpty(profilePath))
                            {
                                break;
                            }

                            var replacements = new Dictionary<string, string>
                        {
                            { "profilePath", profilePath }
                        };

                            translator.printFormatted("options.profiles.delete_confirmation", replacements);
                            Console.Write("\n> ");

                            string? choice = Console.ReadLine();
                            if (choice != "1")
                            {
                                return;
                            }

                            bool success = profileManager.Delete(choosenProfile);
                            if (!success) { return; }

                            translator.print("options.profiles.delete_success");
                            break;
                        }

                    case "4":
                        {
                            return;
                        }
                }
            }
        }
    }
}
