using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1.Generators
{
    public class StringGenerator(string alphabet)
    {
        private readonly string _alphabet = new((alphabet.ToLower() + alphabet.ToUpper()).Distinct().ToArray());
        private readonly Random _random = new();

        public string Generate(int length)
        {
            string result = "";

            for (int i = 0; i < length; i++)
            {
                result += _alphabet[_random.Next(0, _alphabet.Length)];
            }

            return result;
        }
    }
}
