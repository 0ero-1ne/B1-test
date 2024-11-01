using System.IO;
using System.Threading.Tasks;
using Task1.Generators;

namespace Task1
{
    static class Program
    {
        private static readonly DateGenerator dateGenerator = new();
        private static readonly StringGenerator englishStringGenerator = new("abcdefghijklmnopqrstuvwxyz");
        private static readonly StringGenerator russianStringGenerator = new("абвгдеёжзийклмнопрстуфхцчшщъыьэюя");
        private static readonly NumberGenerator numberGenerator = new();

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
                    Generate(args);
                    break;
                case "concat":
                    Concat(args);
                    break;
                default:
                    PrintError("Invalid command");
                    break;
            }
        }

        static void Generate(string[] args) // generate console command
        {
            if (args.Length < 3)
            {
                PrintError("Syntax error.\nGenerate syntax is: task1 generate -o folder_to_save");
                return;
            }

            if (args[1] != "-o" || args[2] == "")
            {
                PrintError("Syntax error.\nGenerate syntax is: task1 generate -o folder_to_save");
                return;
            }

            var directoryPath = args[2];

            if (!Directory.Exists(directoryPath))
            {
                PrintError("No such folder. Check the path");
                return;
            }

            var randomDirectoryName = englishStringGenerator.Generate(20);
            var fullPath = directoryPath + (directoryPath.Last() == '\\' ? $"{randomDirectoryName}\\" : $"\\{randomDirectoryName}\\");
            Directory.CreateDirectory(fullPath);

            Console.WriteLine($"Generating files into {fullPath}");

            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 1; i <= 100; i++)
            {
                var fileName = fullPath + $"file{i}.txt";
                var queue = new Queue<string>();
                object locker = new();

                Parallel.For(0, 5000, (i) => {
                    var strings = "";

                    for (int j = 0; j < 20; j++)
                    {
                        strings += DateGenerator.FormatDate(dateGenerator.Generate()) + "||";
                        strings += englishStringGenerator.Generate(10) + "||";
                        strings += russianStringGenerator.Generate(10) + "||";
                        strings += numberGenerator.GenerateInt(1, 100000000) + "||";
                        strings += NumberGenerator.FormatDouble(numberGenerator.GenerateDouble(1, 20)) + "\n";
                    }

                    lock (locker)
                    {
                        queue.Enqueue(strings);
                    }
                });

                using StreamWriter stream = new(fileName, true);
                foreach (var item in queue)
                {
                    stream.Write(item);
                }
            }

            watch.Stop();

            Console.WriteLine($"100 files were generated successfully in {watch}");
        }

        static void Concat(string[] args) // concat console command
        {
            Console.WriteLine("Concat");
        }

        static void PrintError(string error)
        {
            Console.WriteLine($"Program error: {error}");
        }
    }
}