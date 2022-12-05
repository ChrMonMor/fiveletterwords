using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace fiveletterwords
{
    public class FiveLetterLogic
    {
        static List<string> wordCombos = new List<string>();
        double seconds = 0;
        int fileSize = 0;

        public (List<string> wordCombos, double seconds, int fileSize) WordLogic(string path, int wordLenght, int nWords, bool noAnagrams)
        {
            fileSize = 0;
            wordCombos.Clear();
            seconds = 0;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            try
            {
                string[] lines = File.ReadAllLines(@path);
                fileSize = lines.Length;
                Array.Sort(lines);
                lines = lines.Where(x => x.Length == wordLenght && x.Distinct().Count() == wordLenght).ToArray();
                long[] numbers = Array.Empty<long>();
                if (noAnagrams)
                {
                    (lines, numbers) = RemoveAnagrams(lines);
                }
                else
                {
                    numbers = WordToNumber(lines);
                }
                Parallel.For(0, numbers.Length, i =>
                {
                    long[] result = new long[nWords];
                    result[0] = numbers[i];
                    AndingLoop(ShortArray(numbers, i), numbers[i], lines, 0, result, numbers);
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
            return (wordCombos, stopwatch.Elapsed.TotalSeconds, fileSize);
        }
        static long[] WordToNumber(string[] arr)
        {
            List<long> longs = new List<long>();
            foreach (var item in arr)
            {
                long Bit = 0;
                foreach (char i in item)
                {
                    Bit += 1 << i - 'a';
                }
                longs.Add(Bit);
            }
            return longs.ToArray();
        }
        static (string[], long[]) RemoveAnagrams(string[] arr)
        {
            List<long> longs = new();
            List<string> newArr = arr.ToList();
            foreach (var item in arr)
            {
                long Bit = 0;
                foreach (char i in item)
                {
                    Bit += 1 << i - 'a';
                }
                if (longs.Contains(Bit))
                {
                    newArr.Remove(item);
                    continue;
                }
                longs.Add(Bit);
            }
            return (newArr.ToArray(), longs.ToArray());
        }
        static void AndingLoop(long[] arr, long BitSum, string[] Lines, int SuccessCount, long[] Result, long[] Numbers)
        {
            try
            {
                long[] newArr = arr.Where(x => (x & BitSum) == 0).ToArray();

                if (SuccessCount == Result.Length)
                {
                    wordCombos.Add($"{Lines[Array.IndexOf(Numbers, Result[0])]}, {Lines[Array.IndexOf(Numbers, Result[1])]}, {Lines[Array.IndexOf(Numbers, Result[2])]}, {Lines[Array.IndexOf(Numbers, Result[3])]}, {Lines[Array.IndexOf(Numbers, Result[4])]}");
                    return;
                }
                for (int i = 0; i < newArr.Length; i++)
                {
                    SuccessCount++;
                    if ((newArr[i] & BitSum) == 0)
                    {
                        Result[SuccessCount] = newArr[i];
                        AndingLoop(ShortArray(newArr, i), newArr[i] + BitSum, Lines, SuccessCount, Result, Numbers);
                    }
                    SuccessCount--;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Shorty of Array, remov old used bits n' bobs
        static long[] ShortArray(long[] arr, int StartPos)
        {
            long[] newArr = new long[arr.Length - StartPos];
            for (int i = 0; i < newArr.Length; i++)
            {
                newArr[i] = arr[i + StartPos];
            }
            return newArr;
        }
    }
}
