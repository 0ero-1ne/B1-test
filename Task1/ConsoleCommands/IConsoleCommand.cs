namespace Task1.ConsoleCommands
{
    // Общий интерфейс консольной команды
    public interface IConsoleCommand
    {
        public static void Run(string[] args) => Console.WriteLine("Method not implemented");
        private static void PrintError(string message) => Console.WriteLine("Method not implemented");
        private static void PrintWarning(string message) => Console.WriteLine("Method not implemented");
    }
}
