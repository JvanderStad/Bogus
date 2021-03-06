using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bogus.DataSets;
using Bogus.Extensions;
using Newtonsoft.Json.Linq;

namespace Bogus
{
    /// <summary>
    /// The randomizer. It randoms things.
    /// </summary>
    public class Randomizer
    {
        /// <summary>
        /// Set the random number generator manually with a seed to get reproducible results.
        /// </summary>
        public static Random Seed = new Random();

        internal static Lazy<object> Locker = new Lazy<object>(() => new object(), LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Get an int from 0 to max.
        /// </summary>
        /// <param name="max">Upper bound, inclusive</param>
        /// <returns></returns>
        public int Number(int max)
        {
            return Number(0, max);
        }

        /// <summary>
        /// Get a random sequence of digits
        /// </summary>
        /// <param name="count">How many</param>
        /// <param name="minDigit">minimum digit, inclusive</param>
        /// <param name="maxDigit">maximum digit, inclusive</param>
        /// <returns></returns>
        public int[] Digits(int count, int minDigit = 0, int maxDigit = 9)
        {
            if(maxDigit > 9 || maxDigit < 0) throw new ArgumentException(nameof(maxDigit), "max digit can't be lager than 9 or smaller than 0");
            if(minDigit > 9 || minDigit < 0) throw new ArgumentException(nameof(minDigit), "min digit can't be lager than 9 or smaller than 0");

            var digits = new int[count];
            for(var i = 0; i < count; i++)
            {
                digits[i] = Number(min: minDigit, max: maxDigit);
            }
            return digits;
        }

        /// <summary>
        /// Get an int from min to max.
        /// </summary>
        /// <param name="min">Lower bound, inclusive</param>
        /// <param name="max">Upper bound, inclusive</param>
        /// <returns></returns>
        public int Number(int min = 0, int max = 1)
        {
            //lock any seed access, for thread safety.
            lock(Locker.Value)
            {
                return Seed.Next(min, max + 1);
            }
        }

        /// <summary>
        /// Returns a random even number
        /// </summary>
        /// <param name="min">Lower bound, inclusive</param>
        /// <param name="max">Upper bound, inclusive</param>
        public int Even(int min = 0, int max = 1)
        {
            var result = 0;
            do
            {
                result = Number(min, max);
            } while(result % 2 == 1);
            return result;
        }

        /// <summary>
        /// Returns a random even number
        /// </summary>
        /// <param name="min">Lower bound, inclusive</param>
        /// <param name="max">Upper bound, inclusive</param>
        public int Odd(int min = 0, int max = 1)
        {
            int result = 0;
            do
            {
                result = Number(min, max);
            } while(result % 2 == 0);
            return result;
        }


        /// <summary>
        /// Get a random double.
        /// </summary>
        /// <returns></returns>
        public double Double()
        {
            //lock any seed access, for thread safety.
            lock(Locker.Value)
            {
                return Seed.NextDouble();
            }
        }

        /// <summary>
        /// Get a random boolean
        /// </summary>
        /// <returns></returns>
        public bool Bool()
        {
            return Number() == 0;
        }

        /// <summary>
        /// Get a random array element.
        /// </summary>
        public T ArrayElement<T>(T[] array)
        {
            var r = Number(max: array.Length - 1);
            return array[r];
        }

        /// <summary>
        /// Get a random list item.
        /// </summary>
        public T ListItem<T>(List<T> list)
        {
            var r = Number(max: list.Count - 1);
            return list[r];
        }

        /// <summary>
        /// Helper method to get a random JProperty.
        /// </summary>
        public JToken ArrayElement(JProperty[] props)
        {
            var r = Number(max: props.Length - 1);
            return props[r];
        }

        /// <summary>
        /// Get a random array element.
        /// </summary>
        public string ArrayElement(Array array)
        {
            array = array ?? new[] {"a", "b", "c"};

            var r = Number(max: array.Length - 1);

            return array.GetValue(r).ToString();
        }

        /// <summary>
        /// Helper method to get a random element inside a JArray
        /// </summary>
        public string ArrayElement(JArray array)
        {
            var r = Number(max: array.Count - 1);

            return array[r].ToString();
        }

        /// <summary>
        /// Replaces symbols with numbers. IE: ### -> 283
        /// </summary>
        /// <param name="format"></param>
        /// <param name="symbol"></param>
        public string ReplaceNumbers(string format, char symbol = '#')
        {
            var chars = format.Select(c => c == symbol ? Convert.ToChar('0' + Number(9)) : c)
                .ToArray();

            return new string(chars);
        }

        /// <summary>
        /// Replaces symbols with numbers and letters. # = number, ? = letter, * = number or letter. IE: ###???* -> 283QED4
        /// </summary>
        /// <param name="format"></param>
        public string Replace(string format)
        {
            var chars = format.Select(c =>
                {
                    if(c == '*')
                    {
                        c = Bool() ? '#' : '?';
                    }
                    if(c == '#')
                    {
                        return Convert.ToChar('0' + Number(9));
                    }
                    if(c == '?')
                    {
                        return Convert.ToChar('A' + Number(25));
                    }

                    return c;
                })
                .ToArray();

            return new string(chars);
        }

        /// <summary>
        /// Picks a random Enum of T. Works only with Enums.
        /// </summary>
        /// <typeparam name="T">Must be an Enum</typeparam>
        /// <param name="exclude">Exclude enum values from being returned</param>
        public T Enum<T>(params T[] exclude) where T : struct
        {
            var e = typeof(T);
            if(!e.IsEnum())
                throw new ArgumentException("When calling Enum<T>() with no parameters T must be an enum.");

            var selection = System.Enum.GetNames(e);

            if(exclude.Any())
            {
                var excluded = exclude.Select(ex => System.Enum.GetName(e, ex));
                selection = selection.Except(excluded).ToArray();
            }

            if(!selection.Any())
            {
                throw new ArgumentException("There are no values after exclusion to choose from.");
            }

            var val = this.ArrayElement(selection);

            T picked;
            System.Enum.TryParse(val, out picked);
            return picked;
        }

        /// <summary>
        /// Shuffles an IEnumerable source.
        /// </summary>
        public IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
        {
            List<T> buffer = source.ToList();
            for(var i = 0; i < buffer.Count; i++)
            {
                int j;
                //lock any seed access, for thread safety.
                lock(Locker.Value)
                {
                    j = Seed.Next(i, buffer.Count);
                }
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        /// <summary>
        /// Returns a single word or phrase in English.
        /// </summary>
        public string Word()
        {
            var randomWordMethod = ListItem(WordFunctions.Functions);
            return randomWordMethod();
        }

        /// <summary>
        /// Gets some random words and phrases in English.
        /// </summary>
        /// <param name="count">Number of times to call Word()</param>
        public string Words(int? count = null)
        {
            if(count == null)
                count = Number(1, 3);

            var words = Enumerable.Range(1, count.Value)
                .Select(f => Word()).ToArray(); // lol.

            return string.Join(" ", words);
        }

        /// <summary>
        /// Get a random unique GUID.
        /// </summary>
        public Guid Uuid()
        {
            return Guid.NewGuid();
        }
    }

    public static class WordFunctions
    {
        public static List<Func<string>> Functions = new List<Func<string>>();

        static WordFunctions()
        {
            var commerce = new Commerce();
            var company = new Company();
            var address = new Address();
            var finance = new Finance();
            var hacker = new Hacker();
            var name = new Name();

            Functions.Add(() => commerce.Department());
            Functions.Add(() => commerce.ProductName());
            Functions.Add(() => commerce.ProductAdjective());
            Functions.Add(() => commerce.ProductMaterial());
            Functions.Add(() => commerce.ProductName());
            Functions.Add(() => commerce.Color());

            Functions.Add(() => company.CatchPhraseAdjective());
            Functions.Add(() => company.CatchPhraseDescriptor());
            Functions.Add(() => company.CatchPhraseNoun());
            Functions.Add(() => company.BsAdjective());
            Functions.Add(() => company.BsBuzz());
            Functions.Add(() => company.BsNoun());

            Functions.Add(() => address.StreetSuffix());
            Functions.Add(() => address.County());
            Functions.Add(() => address.Country());
            Functions.Add(() => address.State());
            
            Functions.Add(() => address.StreetSuffix());

            Functions.Add(() => finance.AccountName());
            Functions.Add(() => finance.TransactionType());
            Functions.Add(() => finance.Currency().Description);

            Functions.Add(() => hacker.Noun());
            Functions.Add(() => hacker.Verb());
            Functions.Add(() => hacker.Adjective());
            Functions.Add(() => hacker.IngVerb());
            Functions.Add(() => hacker.Abbreviation());

            Functions.Add(() => name.JobDescriptor());
            Functions.Add(() => name.JobArea());
            Functions.Add(() => name.JobType());
        }
    }

}