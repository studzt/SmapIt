using SmapIt.Utils;

namespace SmapIt.Core
{
    public static class AppCore
    {
        public static Logger Logger { get; } = new Logger();
        public static Logger tempLogger { get; } = new Logger();
    }
}
