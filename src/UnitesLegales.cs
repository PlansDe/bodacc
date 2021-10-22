using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace bodacc
{
    // documentation : https://www.data.gouv.fr/fr/datasets/r/65946639-3150-4492-a988-944e2633531e
    public class SireneUnitesLegales
    {
        const String DB_NAME = "bodacc.db";
        const String SIRENE_DIR = "SIRENE";
        const String LOCAL_ARCHIVE = "stock-unites-legales.zip";
        const String LOCAL_FILENAME = "StockUniteLegale_utf8.csv";
        const String REMOTE_URL = "https://www.data.gouv.fr/fr/datasets/r/825f4199-cadd-486c-ac46-a65a8ea1a047";

        public static void DownloadData(bool forceUpdate = false)
        {
            Console.WriteLine("download unites legales from SIRENE");
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
                System.IO.File.SetLastWriteTimeUtc(local_archive.FullName, DateTime.UtcNow);
            }
        }

        public static void Decompress(bool forceUpdate = false)
        {
            Console.WriteLine("decompress unites legales");
            var local_csv = new FileInfo(Path.Combine(SIRENE_DIR, LOCAL_FILENAME));
            if (forceUpdate || !local_csv.Exists || DateTime.UtcNow - local_csv.LastWriteTimeUtc > TimeSpan.FromDays(1))
            {
                if (local_csv.Exists)
                {
                    File.Delete(local_csv.FullName);
                }

                ZipFile.ExtractToDirectory(Path.Combine(SIRENE_DIR, LOCAL_ARCHIVE), SIRENE_DIR);
                System.IO.File.SetLastWriteTimeUtc(local_csv.FullName, DateTime.UtcNow);
            }
        }

        public static void PopulateDb()
        {
            if (Exists())
            {
                Console.WriteLine("uniteslegales already populated -- aborting");
                return;
            }

            HashSet<int> sirens = new HashSet<int>();
            using (var fileStream = File.OpenRead(Path.Combine(SIRENE_DIR, LOCAL_FILENAME)))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DB_NAME)))
            {
                connection.Open();
                String head = streamReader.ReadLine();
                int keyIndex = 0;
                var labels = head.Split(",");
                var labels_indices = labels.ToDictionary(s => s, s => keyIndex++);
                List<String> lines = new List<string>();
                int ID = 0;
                while (streamReader.ReadLines(100000, lines) != 0)
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = @"
                        INSERT INTO uniteslegales (SIREN, NOM, EFFECTIFS)
                        VALUES (@Siren, @Nom, @Effectifs)
                    ";
                        var pSiren = new SqliteParameter();
                        pSiren.ParameterName = "@Siren";
                        command.Parameters.Add(pSiren);
                        var pNom = new SqliteParameter();
                        pNom.ParameterName = "@Nom";
                        command.Parameters.Add(pNom);
                        var pEff = new SqliteParameter();
                        pEff.ParameterName = "@Effectifs";
                        command.Parameters.Add(pEff);
                        foreach (string line in lines)
                        {
                            var split = line.Split(",");
                            string siren = split[labels_indices["siren"]];
                            if (String.IsNullOrWhiteSpace(siren))
                            {
                                Console.WriteLine("unite legale sans siren -- ignored");
                                continue;
                            }

                            pSiren.Value = int.Parse(siren);
                            pNom.Value = split[labels_indices["denominationUniteLegale"]];
                            pEff.Value = split[labels_indices["trancheEffectifsUniteLegale"]];
                            command.ExecuteNonQuery();
                            ID += 1;
                        }

                        transaction.Commit();
                        Console.Write("\r{0} entries processed", ID);
                    }
                }
            }
        }

        private static bool Exists()
        {
            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DB_NAME)))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM uniteslegales";
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int count = reader.GetInt32(0);
                                if (count > 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }

                    return false;
                }
            }
        }
    }
}