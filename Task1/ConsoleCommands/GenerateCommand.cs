using Task1.Generators;

namespace Task1.ConsoleCommands
{
    public class GenerateCommand : IConsoleCommand
    {
        private static readonly DateGenerator dateGenerator = new();
        private static readonly StringGenerator englishStringGenerator = new("abcdefghijklmnopqrstuvwxyz");
        private static readonly StringGenerator russianStringGenerator = new("абвгдеёжзийклмнопрстуфхцчшщъыьэюя");
        private static readonly NumberGenerator numberGenerator = new();

        public static void Run(string[] args)
        {
            // Так, как приложение консольное, стоит проверить входные параметры
            if (args.Length < 3)
            {
                PrintError("Syntax error.\nGenerate syntax is: task1 generate -o dir_to_save");
                return;
            }

            if (args[1] != "-o" || args[2] == "")
            {
                PrintError("Syntax error.\nGenerate syntax is: task1 generate -o dir_to_save");
                return;
            }

            var directoryPath = args[2];

            if (!Directory.Exists(directoryPath))
            {
                PrintError("No such directory. Check the path");
                return;
            }

            var randomDirectoryName = englishStringGenerator.Generate(20); // случайное название для папки
            var fullPath = directoryPath + (directoryPath.Last() == '\\' ? $"{randomDirectoryName}\\" : $"\\{randomDirectoryName}\\");
            Directory.CreateDirectory(fullPath);

            Console.WriteLine($"Generating files into {fullPath}");

            var watch = System.Diagnostics.Stopwatch.StartNew(); // Для подсчёта времени выполнения алгоритма

            for (int i = 1; i <= 100; i++)
            {
                var fileName = fullPath + $"file{i}.txt";
                var queue = new Queue<string>();
                object locker = new();

                // Распараллелим вычисления для ускорения создания контента для файла
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

        private static void PrintError(string message) => Console.WriteLine($"Generate error: {message}");

        private static void PrintWarning(string message) => Console.WriteLine($"Generate warning: {message}");

    }
}
