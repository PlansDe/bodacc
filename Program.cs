using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Data.Sqlite;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Diagnostics;

namespace bodacc
{

    class Program
    {
        const String DB_NAME = "bodacc.db";
        const String BODACC_DIR = "BODACC";
        const String REMOTE_FILE_FORMAT_2021 = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/{0:D4}/PCL_BXA{0:D4}{1:D4}.taz";
        // 2017 - 2021
        const String REMOTE_FILE_FORMAT_HISTORY = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/FluxHistorique/{0:D4}/PCL_BXA{0:D4}{1:D4}.taz";
        // 2008 - 2021
        const String REMOTE_ARCHIVE_HISTO = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/FluxHistorique/BODACC_{0:D4}.tar";
        static void Main(string[] args)
        {
            //DownloadData(2008);
            //DecompressData();
            PopulateDB(BODACC_DIR);
        }

        static void DownloadData(int from_year)
        {

            WebClient client = new WebClient();
            Console.WriteLine("Downloading data from {0} to {1} included", from_year, DateTime.Now.Year);
            for (int year = from_year; year <= DateTime.Now.Year; ++year)
            {
                Console.WriteLine("Downloading year {0}", year);
                if (year >= 2017 && year <= DateTime.Now.Year)
                {
                    DownloadYearlyData_Recent(client, year);
                }
                else
                {
                    DownloadYearlyData_Old(client, year);
                }
            }
        }

        static void DownloadYearlyData_Old(WebClient client, int year)
        {
            String local_dir = Path.Combine(BODACC_DIR, year.ToString("D4"));
            if (!Directory.Exists(local_dir))
            {
                Directory.CreateDirectory(local_dir);
            }
            String long_file_name = Path.Combine(local_dir, String.Format("BODACC_{0:D4}.tar", year));
            if (!File.Exists(long_file_name))
            {
                client.DownloadFile(String.Format(REMOTE_ARCHIVE_HISTO, year), long_file_name);
            }
        }

        static void DownloadYearlyData_Recent(WebClient client, int year)
        {
            String remote_file_format = (year < 2021 && year >= 2017) ? REMOTE_FILE_FORMAT_HISTORY : REMOTE_FILE_FORMAT_2021;
            String local_dir = Path.Combine(BODACC_DIR, year.ToString("D4"));
            if (!Directory.Exists(local_dir))
            {
                Directory.CreateDirectory(local_dir);
            }
            int file_id = 1;
            var existingFiles = Directory.GetFiles(local_dir, "*.taz");
            Func<String, bool> override_condition = (String s) => false;
            if (existingFiles.Any())
            {
                var last_file = new FileInfo(existingFiles.OrderByDescending(f => f).First());
                if (last_file.Exists)
                {
                    override_condition = (String s) => year == DateTime.Now.Year && s == last_file.Name;
                }
            }

            while (true)
            {
                var file_name = String.Format("PCL_BXA{0:D4}{1:D4}.taz", year, file_id);
                var long_file_name = Path.Combine(local_dir, file_name);
                if (!File.Exists(long_file_name) || override_condition(file_name))
                {
                    var remote_file = String.Format(remote_file_format, year, file_id);
                    try
                    {
                        client.DownloadFile(remote_file, long_file_name);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("download error for {0}", remote_file);
                        Console.Error.WriteLine(e.Message);
                        break;
                    }
                }

                file_id++;
            }
        }


        static void DecompressData()
        {
            foreach (var directory in Directory.EnumerateDirectories(BODACC_DIR).OrderBy(d => d))
            {
                Console.WriteLine("extracting data in {0}", directory);
                var di = new DirectoryInfo(directory);
                int year = int.Parse(di.Name);
                if (year < 2017)
                {
                    DecompressOld(directory);
                    DecompressRecent(directory);
                }
                else
                {
                    DecompressRecent(directory);
                }

                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    var ff = new FileInfo(file);
                    if (file.EndsWith(".tar"))
                        continue;
                    if (ff.Name.StartsWith("PCL_BXA"))
                        continue;
                    File.Delete(file);
                }
            }
        }

        static void PopulateDB(string directory)
        {
            ulong ID = 1;
            foreach (String subDirectory in Directory.GetDirectories(directory).OrderBy(d => d))
            {
                int year = int.Parse(new DirectoryInfo(subDirectory).Name);
                PopulateDBYear(subDirectory, year, ref ID);
            }
        }

        static void PopulateDBYear(string directory, int year, ref ulong ID)
        {
            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DB_NAME)))
            {
                connection.Open();

                foreach (var file in Directory.GetFiles(directory, "*.xml"))
                {
                    //Console.Write("\rinserting file {0} -- ID = {1}", file, ID);
                    DBProcessFile(file, connection, year, ref ID);
                }
            }

            Console.WriteLine("\r year {0} done", year);
        }

        static void DecompressOld(String directory)
        {
            var tar = Directory.GetFiles(directory, "*.tar");
            foreach (var file in tar)
            {
                Decompress(new FileInfo(file), "xf");
            }

            // clean (archive structures change year after year)
            foreach (var file in Directory.EnumerateFiles(directory, "PCL_BXA*", SearchOption.AllDirectories))
            {
                var source = new FileInfo(file);
                var dest = Path.Combine(directory, source.Name);
                File.Move(file, dest, true);
            }

            foreach (var sub_directory in Directory.EnumerateDirectories(directory))
            {
                Directory.Delete(sub_directory, true);
            }
        }

        public static void DecompressRecent(String directory)
        {
            var taz = Directory.GetFiles(directory, "PCL_BXA*");
            foreach (var file in taz.Where(f => f.EndsWith(".taz")))
            {
                Decompress(new FileInfo(file), "xzf");
            }
        }

        public static void Decompress(FileInfo fileToDecompress, string options)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = fileToDecompress.DirectoryName,
                FileName = "/bin/bash",
                Arguments = String.Format(" -c \"tar {0} {1}\"", options, fileToDecompress.Name),
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process.Start(startInfo).WaitForExit();
        }

        static void DBProcessFile(String file, SqliteConnection connection, int year, ref ulong ID)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                                INSERT INTO annonces (ID, NUMERO, DATE, CODEPOSTAL,VILLE,NATURE,RCS,TYPE,FORMEJURIDIQUE)
                                VALUES (@ID,@Numero,@Date,@CodePostal,@Ville,@Nature,@Rcs,@Type,@FormeJuridique)
                            ";

                var idParam = new SqliteParameter();
                idParam.ParameterName = "@ID";
                command.Parameters.Add(idParam);
                var numeroParam = new SqliteParameter();
                numeroParam.ParameterName = "@Numero";
                command.Parameters.Add(numeroParam);
                var dateParam = new SqliteParameter();
                dateParam.ParameterName = "@Date";
                command.Parameters.Add(dateParam);
                var codePostalParam = new SqliteParameter();
                codePostalParam.ParameterName = "@CodePostal";
                command.Parameters.Add(codePostalParam);
                var villeParam = new SqliteParameter();
                villeParam.ParameterName = "@Ville";
                command.Parameters.Add(villeParam);
                var natureParam = new SqliteParameter();
                natureParam.ParameterName = "@Nature";
                command.Parameters.Add(natureParam);
                var rcsParam = new SqliteParameter();
                rcsParam.ParameterName = "@Rcs";
                command.Parameters.Add(rcsParam);
                var typeParam = new SqliteParameter();
                typeParam.ParameterName = "@Type";
                command.Parameters.Add(typeParam);
                var formeParam = new SqliteParameter();
                formeParam.ParameterName = "@FormeJuridique";
                command.Parameters.Add(formeParam);

                XmlSerializer serializer = new XmlSerializer(typeof(PCL_REDIFF));

                using (XmlReader reader = XmlReader.Create(file))
                {
                    PCL_REDIFF bulletin;
                    try
                    {
                        bulletin = (PCL_REDIFF)serializer.Deserialize(reader);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("cannot deserialize file {0}", file);
                        Console.Error.WriteLine(e.Message);
                        return;
                    }

                    foreach (var annonce in bulletin.Annonces.Annonce)
                    {
                        var codePostal = "";
                        var ville = "";
                        if (annonce.Adresse != null && annonce.Adresse.Any())
                        {
                            var france = annonce.Adresse.First().France;
                            if (france != null)
                            {
                                if (france.CodePostal != null)
                                    codePostal = annonce.Adresse.First().France.CodePostal;
                                if (france.Ville != null)
                                    ville = france.Ville;
                            }
                        }

                        var rcs = annonce.NumeroImmatriculation.Any() ? annonce.NumeroImmatriculation.First().NumeroIdentificationRCS : "non inscrit";
                        var previous = annonce.ParutionAvisPrecedent == null ? "" : annonce.ParutionAvisPrecedent.NumeroAnnonce;
                        var type = annonce.TypeAnnonce.Creation != null ? "creation" :
                                       (annonce.TypeAnnonce.Rectificatif != null ? "rectificatif" : "");

                        var date = "";
                        var french = CultureInfo.GetCultureInfo("fr-FR");
                        var styles = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AllowTrailingWhite;
                        if (annonce.Jugement != null && !String.IsNullOrWhiteSpace(annonce.Jugement.Date))
                        {
                            date = "";
                            var input = annonce.Jugement.Date
                                .Replace("1er", "1")
                                .Replace('\u00ef', ' ')
                                .Replace('\u00bf', ' ')
                                .Replace('\u00bd', ' ')
                                .Replace("f   evrier", "février")
                                .Replace("ao   t", "août")
                                .Replace("d   cembre", "décembre");
                            DateTime parsed_date;
                            if (!DateTime.TryParseExact(input, "yyyy-mm-dd", CultureInfo.InvariantCulture, styles, out parsed_date))
                            {
                                if (!DateTime.TryParseExact(input, "d MMMM yyyy", french, styles, out parsed_date))
                                {
                                    Console.WriteLine("cannot parse date : " + annonce.Jugement.Date);
                                }
                            }

                            date = parsed_date.ToString("yyyy-MM-dd");
                        }

                        var nature = "";
                        if (annonce.Jugement != null && annonce.Jugement.Nature != null)
                        {
                            if (annonce.Jugement.Nature != null)
                                nature = annonce.Jugement.Nature;
                        }

                        var forme = "";
                        if (annonce.PersonneMorale != null && annonce.PersonneMorale.Any())
                        {
                            if (annonce.PersonneMorale.First().FormeJuridique != null)
                                forme = annonce.PersonneMorale.First().FormeJuridique;
                        }

                        idParam.Value = ID;
                        numeroParam.Value = annonce.NumeroAnnonce.ToLowerInvariant().Trim();
                        dateParam.Value = date;
                        codePostalParam.Value = codePostal;
                        villeParam.Value = ville;
                        natureParam.Value = nature.ToLowerInvariant();
                        rcsParam.Value = rcs.Replace(" ", "");
                        typeParam.Value = type.ToLowerInvariant();
                        formeParam.Value = forme.ToLowerInvariant();
                        ID += 1;
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine("cannot insert ID : {0} for numero : {1}", ID, annonce.NumeroAnnonce);
                            Console.Error.WriteLine(e.Message);
                        }
                    }

                    transaction.Commit();
                }
            }
        }
    }
}