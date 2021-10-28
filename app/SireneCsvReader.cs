using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bodacc
{
    public static class SireneCsvReader
    {
        public static int ReadLines(this StreamReader streamReader, int count, List<String> lines)
        {
            int i = 0;
            lines.Clear();
            String line;
            while ((line = streamReader.ReadLine()) != null && i < count)
            {
                lines.Add(line);
                i++;
            }

            return i;
        }

        public static string[] ReadLine(string input)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            List<String> result = new List<string>();
            bool is_reading_string = false;
            while (true)
            {
                if (i == input.Length)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                    return result.ToArray();
                }

                var c = input[i];
                if (c == ',' && !is_reading_string)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                    i++;
                    continue;
                }
                else if (c == '"' && is_reading_string)
                {
                    sb.Append(c);
                    is_reading_string = false;
                    i++;
                    continue;
                }
                else if (c == '"' && !is_reading_string)
                {
                    sb.Append(c);
                    is_reading_string = true;
                    i++;
                    continue;
                }
                else
                {
                    sb.Append(c);
                    i++;
                    continue;
                }
            }
        }
    }
}