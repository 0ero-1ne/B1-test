using Task1.ConsoleCommands;

namespace Task1
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintError("No arguments");
                return;
            }

            switch (args[0])
            {
                case "generate":
                    GenerateCommand.Run(args);
                    break;
                case "concat":
                    ConcatCommand.Run(args);
                    break;
                case "import":
                    ImportCommand.Run(args);
                    break;
                default:
                    PrintError("Invalid command");
                    break;
            }
        }

        static void PrintError(string error) => Console.WriteLine($"Program error: {error}");
    }
}