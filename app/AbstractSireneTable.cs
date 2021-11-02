using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace bodacc
{
    public abstract class AbstractSireneTable : AbstractLargeTable<string>
    {
        protected Dictionary<String, int> label_indices;
        protected String[] labels;


        protected const String SIRENE_DIR = "SIRENE";
        protected abstract String LOCAL_ARCHIVE { get; }
        protected abstract String LOCAL_FILENAME { get; }
        protected abstract String REMOTE_URL { get; }

        public void DownloadData(bool forceUpdate = false)
        {
            Console.WriteLine($"download {TABLE_NAME} from SIRENE");
            if (!Directory.Exists(SIRENE_DIR))
            {
                Directory.CreateDirectory(SIRENE_DIR);
            }
            var local_archive = new FileInfo(Path.Combine(SIRENE_DIR, LOCAL_ARCHIVE));
            if (forceUpdate || !local_archive.Exists || DateTime.UtcNow - local_archive.LastWriteTimeUtc > TimeSpan.FromDays(7))
            {
                if (local_archive.Exists)
                {
                    local_archive.Delete();
                }

                new WebClient().DownloadFile(REMOTE_URL, local_archive.FullName);
                System.IO.File.SetLastWriteTimeUtc(local_archive.FullName, DateTime.UtcNow);
            }
        }

        public bool Decompress(bool forceUpdate = false)
        {
            Console.WriteLine($"decompress {TABLE_NAME}");
            var local_csv = new FileInfo(Path.Combine(SIRENE_DIR, LOCAL_FILENAME));
            if (forceUpdate || !local_csv.Exists || DateTime.UtcNow - local_csv.LastWriteTimeUtc > TimeSpan.FromDays(7))
            {
                if (local_csv.Exists)
                {
                    File.Delete(local_csv.FullName);
                }

                ZipFile.ExtractToDirectory(Path.Combine(SIRENE_DIR, LOCAL_ARCHIVE), SIRENE_DIR);
                System.IO.File.SetLastWriteTimeUtc(local_csv.FullName, DateTime.UtcNow);
                return true;
            }

            return false;
        }

        public void PopulateDb(bool forceUpdate = false)
        {
            Console.WriteLine($"populate {TABLE_NAME}");
            if (Exists())
            {
                if (forceUpdate)
                {
                    Delete();
                }
                else
                {
                    Console.WriteLine($"{TABLE_NAME} already exists -- aborting");
                    return;
                }
            }

            using (var fileStream = File.OpenRead(Path.Combine(SIRENE_DIR, LOCAL_FILENAME)))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
            {
                String head = streamReader.ReadLine();
                int keyIndex = 0;
                this.labels = head.Split(",");
                this.label_indices = this.labels.ToDictionary(s => s, s => keyIndex++);

                List<String> lines = new List<string>();
                while (streamReader.ReadLines(100000, lines) != 0)
                {
                    Commit(lines);
                    INSERT_COUNT += lines.Count;
                    Console.Write($"\r{INSERT_COUNT} rows inserted");
                }
                Console.WriteLine($"\n{TABLE_NAME} fully populated");
            }
        }

    }
}