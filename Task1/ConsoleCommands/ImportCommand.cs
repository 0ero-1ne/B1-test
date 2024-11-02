using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Task1.ConsoleCommands
{
    public class ImportCommand : IConsoleCommand
    {
        public static void Run(string[] args)
        {
            if (args.Length < 3)
            {
                PrintError("Syntax error.\nImport syntax is: task1 import -f file_path");
                return;
            }

            if (args[1] != "-f" || args[2] == "")
            {
                PrintError("Syntax error.\nImport syntax is: task1 import -f file_path");
                return;
            }

            var filePath = args[2];

            if (!File.Exists(filePath))
            {
                PrintError("No such file. Check the path");
                return;
            }

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using SqlConnection connection = new(connectionString);

            try
            {
                connection.Open();
                var queue = new Queue<string>();
                
                using StreamReader reader = new(filePath, System.Text.Encoding.UTF8, true, 4096);
                var str = reader.ReadLine();
                while (str != null && str != "\n")
                {
                    queue.Enqueue(str);
                    str = reader.ReadLine();
                }

                var watch = System.Diagnostics.Stopwatch.StartNew();
                InsertToDB(connectionString, queue);
                watch.Stop();
                
                Console.WriteLine($"Done in {watch}");
                connection.Close();
            }
            catch (SqlException e)
            {
                PrintError($"SQL Error: {e.Message}");
                connection.Close();
                return;
            }
        }

        private static void InsertToDB(string connectionString, Queue<string> data)
        {
            string query = "set dateformat dmy;" +
                "insert into useful_data (date, english_string, russian_string, integer_number, float_number)" +
                "values (@date, @eng, @rus, @int_number, @double_number);";

            using SqlConnection connection = new(connectionString);


            connection.Open(); // Не будет нового соединения, а возьмётся из пула

            // Будем вставлять транзакцией
            // Если одна строка не вставится, то всё не вставится
            SqlTransaction transaction = connection.BeginTransaction();

            int rowNumber = 1;

            foreach (var item in data)
            {
                var dataFromString = item.Split("||");

                // Такой способ вставки данных является безопасным и защищает от sql-инъекций
                using SqlCommand command = new(query, connection);
                command.Transaction = transaction;

                command.Parameters.Add("@date", SqlDbType.DateTime).Value = DateTime.ParseExact(
                    dataFromString[0],
                    "dd.mm.yyyy",
                    new CultureInfo("ru-RU")
                );
                command.Parameters.Add("@eng", SqlDbType.Char, 10).Value = dataFromString[1];
                command.Parameters.Add("@rus", SqlDbType.NChar, 10).Value = dataFromString[2];
                command.Parameters.Add("@int_number", SqlDbType.Int).Value = int.Parse(dataFromString[3]);
                command.Parameters.Add("@double_number", SqlDbType.Float).Value = float.Parse(dataFromString[4]);

                try
                {
                    var result = command.ExecuteNonQuery();

                    if (result == 0)
                    {
                        PrintError($"Transaction failed. Stopping the program...");
                        return;
                    }

                    Console.WriteLine($"Row {rowNumber++}/{data.Count} inserted");
                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    PrintError($"SQL Error: {e.Message}");
                    return;
                }
                catch (Exception e)
                {
                    PrintError($"General Error: {e.Message} {e.StackTrace}");
                    return;
                }
            }

            transaction.Commit();
        }

        private static void PrintError(string message) => Console.WriteLine($"Import error: {message}");

        private static void PrintWarning(string message) => Console.WriteLine($"Import warning: {message}");
    }
}
