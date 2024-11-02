namespace Task1.ConsoleCommands
{
    public class ConcatCommand : IConsoleCommand
    {
        public static void Run(string[] args)
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

            // Убедимся, что не стоит лишних символов "\"
            directoryPath = directoryPath.Last() == '\\' ? directoryPath : directoryPath + '\\';

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
                    string fileName = directoryPath + $"file{f}.txt";
                    try
                    {
                        using StreamReader reader = new(fileName, System.Text.Encoding.UTF8, true, 4096);
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
                    catch (IOException)
                    {
                        Console.WriteLine($"Warning: {fileName} not found");
                    }
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

        private static void PrintError(string message) => Console.WriteLine($"Concat error: ${message}");

        private static void PrintWarning(string message) => Console.WriteLine($"Concat warning: {message}");
    }
}
