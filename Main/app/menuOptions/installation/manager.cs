using SmapIt.Utils;
using SmapIt.App.menuOptions.installation;

namespace SmapIt.app.menuOptions
{
    internal class InstallManager
    {
        public async Task manager()
        {
            var translator = new Translator();
            
            while (true)
            {
                translator.print("options.install_manager");
                Console.Write("\n> ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                    {
                        await new Install().install();
                        break;
                    }

                    case "2":
                    {
                        new Uninstall().uninstall();
                        break;
                    }

                    case "3":
                        {
                            break;
                        }

                    case "4":
                        {
                            return;
                        }

                    default:
                        {
                            translator.print("main_menu.invalid_input");
                            Console.WriteLine();
                            break;
                        }
                }
            }
        }
    }
}