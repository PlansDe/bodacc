using System;
using System.Collections.Generic;
using System.IO;

namespace bodacc
{
    public static class StreamReaderExtensions
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
    }
}