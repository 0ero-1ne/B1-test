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

            var randomDirectoryName = englishStringGenerator.Generate(20);
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

        static void Concat(string[] args) // concat console command
        {
            if (args.Length < 5)
            {
                PrintError("Syntax error.\nGenerate syntax is: task1 concat -f files_dir -p pattern");
                Console.WriteLine("Leave pattern like \"\" for empty string");
                return;
            }

            if (args[1] != "-f" || args[2] == "" || args[3] != "-p")
            {
                PrintError("Syntax error.\nGenerate syntax is: task1 concat -f files_dir -p pattern");
                Console.WriteLine("Leave pattern like \"\" for empty string");
                return;
            }

            var directoryPath = args[2];

            if (!Directory.Exists(directoryPath))
            {
                PrintError("No such directory. Check the path");
                return;
            }
            directoryPath = directoryPath.Last() == '\\' ? directoryPath : directoryPath + '\\'; // Убедимся, что не стоит лишних символов "\"

            // Contains("") вернёт true всегда
            // Так, как символ октоторпа в файле не используется, то заменим пустую строку на него
            // Также надо учитывать, что паттерн чувствителен к регистру
            var pattern = args[4] == "" ? "#" : args[4]; 

            var content = new Queue<string>();
            long deletedRows = 0;
            object locker = new();

            Console.WriteLine($"Concatenating files into {directoryPath}concatenated.txt");

            var watch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 5, (i) =>
            {
                var queue = new Queue<string>();
                for (int f = i * 20 + 1; f <= i * 20 + 20; f++)
                {
                    using StreamReader reader = new(directoryPath + $"file{f}.txt", System.Text.Encoding.UTF8, true, 4096);
                    var str = reader.ReadLine();
                    while (str != null && str != "\n")
                    {
                        if (str.Contains(pattern))
                        {
                            deletedRows++;
                        }
                        else
                        {
                            queue.Enqueue(str + '\n');
                        }
                        str = reader.ReadLine();
                    }
                    reader.Close();
                }
                lock (locker)
                {
                    foreach (var item in queue)
                    {
                        content.Enqueue(item);
                    }
                }
            });

            using StreamWriter writer = new(directoryPath + $"concatenated.txt", false);
            foreach (var item in content)
            {
                writer.Write(item);
            }
            watch.Stop();

            Console.WriteLine($"Concatenating done in {watch}");
            Console.WriteLine($"{deletedRows} rows where deleted");
        }

        static void PrintError(string error)
        {
            Console.WriteLine($"Program error: {error}");
        }
    }
}