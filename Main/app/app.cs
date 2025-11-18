using SmapIt.app.menuOptions;
using SmapIt.App.menuOptions;
using SmapIt.Utils;

namespace SmapIt.App

{
    internal class App
    {
        public async Task run()
        {
            // Main
            var translator = new Translator();

            while (true)
            {
                Console.WriteLine();
                translator.print("main_menu.main");
                Console.WriteLine("");

                Console.Write("> ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await new Start().start();
                        break;

                    case "2":
                        await new InstallManager().manager();
                        break;

                    case "3":
                        new Profiles().profiles();
                        break;

                    case "4":
                        new Settings().settings();
                        break;

                    default:
                        translator.print("main_menu.invalid_input");
                        break;
                }
            }
        }
    }
}
