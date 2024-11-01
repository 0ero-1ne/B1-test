namespace Task1.Generators
{
    public class DateGenerator
    {
        private readonly Random _random = new();

        public DateTime Generate()
        {
            long fiveYearsInMs = (long)5 * 365 * 24 * 60 * 60 * 1000;
            long currentTimeInMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var randomDateInMs = currentTimeInMs - _random.NextInt64() % fiveYearsInMs;
            var randomDate = new DateTime(1970, 1, 1).AddMilliseconds(randomDateInMs);

            return randomDate;
        }

        public static string FormatDate(DateTime date)
        {
            var day = date.Day.ToString().PadLeft(2, '0');
            var month = date.Month.ToString().PadLeft(2, '0');
            var year = date.Year;

            return $"{day}.{month}.{year}";
        }
    }
}
