using SmapIt.Utils;

namespace SmapIt.app.menuOptions.installation
{
    internal class InstallOptions
    {
        public void installOptions()
        {
            Translator translator = new Translator();

            while (true)
            {
                translator.print("options.install_options.main");
                Console.Write("\n > ");
                string? input = Console.ReadLine();

                switch (input)
                {

                }
            }
        }
    }
}
