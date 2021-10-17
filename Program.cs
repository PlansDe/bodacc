using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Data.Sqlite;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace bodacc
{

    class Program
    {
        const String DB_NAME = "bodacc.db";
        const String BODACC_DIR = "BODACC";
        const String REMOTE_FILE_FORMAT_2021 = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/{0:D4}/PCL_BXA{0:D4}{1:D4}.taz";
        const String REMOTE_FILE_FORMAT_HISTORY = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/FluxHistorique/{0:D4}/PCL_BXA{0:D4}{1:D4}.taz";

        static void Main(string[] args)
        {
            DownloadData(new int[] { 2017, 2018, 2019, 2020, 2021 });
            PopulateDB(BODACC_DIR);
        }

        static void DownloadData(int[] years)
        {

            WebClient client = new WebClient();
            foreach (var year in years)
            {
                DownloadYearlyData(client, year);
            }
        }

        static void DownloadYearlyData(WebClient client, int year)
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
                        Decompress(new FileInfo(long_file_name));
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

        static void PopulateDB(string directory)
        {
            ulong ID = 1;
            foreach (String subDirectory in Directory.GetDirectories(directory).OrderByDescending(d => d))
            {
                PopulateDBYear(subDirectory, ref ID);
            }
        }

        static void PopulateDBYear(string directory, ref ulong ID)
        {
            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DB_NAME)))
            {
                connection.Open();

                foreach (var file in Directory.GetFiles(directory, "*.xml"))
                {
                    Console.Write("\rinserting file {0} -- ID = {1}", file, ID);
                    DBProcessFile(file, connection, ref ID);
                }
            }
        }



        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream fileStream = fileToDecompress.OpenRead())
            {
                var reader = ReaderFactory.Open(fileStream);

                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                        reader.WriteEntryToDirectory(fileToDecompress.DirectoryName);

                }
            }
        }

        static void DBProcessFile(String file, SqliteConnection connection, ref ulong ID)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            XmlSerializer serializer = new XmlSerializer(typeof(PCL_REDIFF));
            using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                                INSERT INTO annonces (ID, NUMERO, DATE, ADDRESS, NATURE, RCS, TYPE, PREVIOUS, COMPLEMENT)
                                VALUES (@ID,@Numero,@Date,@Address,@Nature,@Rcs,@Type,@Previous,@Complement)
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
                var addressParam = new SqliteParameter();
                addressParam.ParameterName = "@Address";
                command.Parameters.Add(addressParam);
                var natureParam = new SqliteParameter();
                natureParam.ParameterName = "@Nature";
                command.Parameters.Add(natureParam);
                var rcsParam = new SqliteParameter();
                rcsParam.ParameterName = "@Rcs";
                command.Parameters.Add(rcsParam);
                var typeParam = new SqliteParameter();
                typeParam.ParameterName = "@Type";
                command.Parameters.Add(typeParam);
                var previousParam = new SqliteParameter();
                previousParam.ParameterName = "@Previous";
                command.Parameters.Add(previousParam);
                var complementParam = new SqliteParameter();
                complementParam.ParameterName = "@Complement";
                command.Parameters.Add(complementParam);

                using (XmlReader reader = XmlReader.Create(file))
                {
                    PCL_REDIFF bulletin = (PCL_REDIFF)serializer.Deserialize(reader);
                    foreach (var annonce in bulletin.Annonces.Annonce)
                    {
                        var address = "";
                        try
                        {
                            String.Join(Environment.NewLine,
                          annonce.Adresse.Select((a) =>
                              String.Format($"{a.France.NumeroVoie} {a.France.NomVoie}, {a.France.CodePostal} {a.France.Ville} {a.France.Ville}")));
                        }
                        catch (NullReferenceException)
                        {
                        }

                        var rcs = annonce.NumeroImmatriculation.Any() ? annonce.NumeroImmatriculation.First().NumeroIdentificationRCS : "non inscrit";
                        var previous = annonce.ParutionAvisPrecedent == null ? "" : annonce.ParutionAvisPrecedent.NumeroAnnonce;
                        var type = annonce.TypeAnnonce.Creation != null ? "creation" :
                                       (annonce.TypeAnnonce.Rectificatif != null ? "rectificatif" : "");

                        var date = "";
                        if (annonce.Jugement != null && !String.IsNullOrWhiteSpace(annonce.Jugement.Date))
                        {
                            date = annonce.Jugement.Date;
                        }
                        var nature = annonce.Jugement != null ? annonce.Jugement.Nature : "";
                        var complement = annonce.Jugement != null ? annonce.Jugement.ComplementJugement : "";

                        idParam.Value = ID;
                        numeroParam.Value = annonce.NumeroAnnonce.ToLowerInvariant().Trim();
                        dateParam.Value = date;
                        addressParam.Value = address;
                        natureParam.Value = nature;
                        rcsParam.Value = rcs.Replace(" ", "");
                        typeParam.Value = type;
                        previousParam.Value = previous;
                        complementParam.Value = complement;
                        ID += 1;
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            Console.Error.WriteLine("cannot insert ID : {0} for numero : {1}", ID, annonce.NumeroAnnonce);
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }
}
