using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace bodacc
{
    public class SireneImport
    {
        const String SIRENE_DIR = "SIRENE";
        const String LOCAL_ARCHIVE = "stock-unite-legale.zip";
        const String LOCAL_FILENAME = "StockEtablissement_utf8.csv";

        const String REMOTE_URL = "https://www.data.gouv.fr/fr/datasets/r/0651fb76-bcf3-4f6a-a38d-bc04fa708576";

        public static void DownloadData(bool forceUpdate = false)
        {
            if (!Directory.Exists(SIRENE_DIR))
            {
                Directory.CreateDirectory(SIRENE_DIR);
            }
            var local_archive = new FileInfo(Path.Combine(SIRENE_DIR, LOCAL_ARCHIVE));
            if (forceUpdate || !local_archive.Exists || DateTime.UtcNow - local_archive.LastWriteTimeUtc > TimeSpan.FromDays(1))
            {
                if (local_archive.Exists)
                {
                    local_archive.Delete();
                }

                new WebClient().DownloadFile(REMOTE_URL, local_archive.FullName);
                local_archive.LastWriteTimeUtc = DateTime.UtcNow;
            }
        }

        public static void Decompress(bool forceUpdate = false)
        {
            var local_csv = new FileInfo(Path.Combine(SIRENE_DIR, LOCAL_FILENAME));
            if (forceUpdate || !local_csv.Exists || DateTime.UtcNow - local_csv.LastWriteTimeUtc > TimeSpan.FromDays(1))
            {
                ZipFile.ExtractToDirectory(Path.Combine(SIRENE_DIR, LOCAL_ARCHIVE), SIRENE_DIR);
                local_csv.LastWriteTimeUtc = DateTime.UtcNow;
            }
        }

        public static void PopulateDb()
        {
            using (var fileStream = File.OpenRead(Path.Combine(SIRENE_DIR, LOCAL_FILENAME)))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
            {
                String head = streamReader.ReadLine();
                var labels = head.Split(",").ToDIctiona
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var split = line.Split(",");

                }
            }
        }
    }
}