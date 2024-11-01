namespace Task1.Generators
{
    public class NumberGenerator
    {
        private readonly Random _random = new();

        public int GenerateInt(int from, int to) => _random.Next(from, to);

        public double GenerateDouble(double from, double to) => _random.NextDouble() * (to - from) + from;

        public static string FormatDouble(double number) => string.Format("{0:f8}", number);
    }
}
