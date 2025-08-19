using main.classes;
using main.app.menuOptions;

namespace main.app

{
    internal class App
    {
        public async Task run()
        {
            // Main
            var translator = new Translator();

            while (true)
            {
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
                        await new Install().install();
                        break;

                    case "3":
                        new Uninstall().uninstall();
                        break;

                    case "4":
                        new Settings().settings();
                        break;
                    default:
                        translator.print("main_menu.invalid_input");
                        Console.WriteLine();
                        break;
                }
            }
        }
    }
}
