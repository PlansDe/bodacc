using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace convert
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<DateTime, int> buckets = new Dictionary<DateTime, int>();
            foreach (var line in File.ReadAllLines("queries/insuffisancedactifs.csv").Skip(1))
            {
                var datestr = line.Split(",", StringSplitOptions.RemoveEmptyEntries)[0];
                if (!DateTime.TryParseExact(datestr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out DateTime date))
                {
                    Console.Error.WriteLine("cannot parse " + datestr + " in line " + line);
                    continue;
                }

                var startOfMonth = new DateTime(date.Year, date.Month, 1);
                if (!buckets.ContainsKey(startOfMonth))
                {
                    buckets.Add(startOfMonth, 0);
                }
                buckets[startOfMonth] += 1;
            }

            File.WriteAllLines("converted.csv", buckets.Select(kvp =>
            kvp.Key.ToString("yyyy-MM-dd") + "," + kvp.Value));
        }
    }
}