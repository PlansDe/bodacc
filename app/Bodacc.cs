using System;

using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Text;
using Npgsql;

namespace bodacc
{
    public class BodaccImport : AbstractLargeTable<Annonce>
    {
        private State state;

        public BodaccImport()
        {
            state = new State();
        }

        const String BODACC_DIR = "BODACC";
        const String REMOTE_FILE_FORMAT_2021 = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/{0:D4}/PCL_BXA{0:D4}{1:D4}.taz";
        // 2017 - 2021
        const String REMOTE_FILE_FORMAT_HISTORY = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/FluxHistorique/{0:D4}/PCL_BXA{0:D4}{1:D4}.taz";
        // 2008 - 2021
        const String REMOTE_ARCHIVE_HISTO = "https://echanges.dila.gouv.fr/OPENDATA/BODACC/FluxHistorique/BODACC_{0:D4}.tar";

        String parution;

        String last_year = State.Bodacc.LastParution.Substring(0, 4);

        protected override string TABLE_NAME => "annonces";

        protected override string HEADER => "PARUTION,NUMERO,DATE, CODEPOSTAL,VILLE,NATURE,RCS,TYPE,FORMEJURIDIQUE, PREVIOUS_PARUTION,PREVIOUS_NUMERO";

        public void DownloadData(int from_year)
        {
            WebClient client = new WebClient();
            Console.WriteLine("Downloading data from {0} to {1} included", from_year, DateTime.Now.Year);
            for (int year = Math.Max(int.Parse(last_year), from_year); year <= DateTime.Now.Year; ++year)
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

        public void DecompressData()
        {
            foreach (var directory in Directory.EnumerateDirectories(BODACC_DIR).OrderBy(d => d))
            {
                var directoryName = new DirectoryInfo(directory).Name;
                if (directoryName.CompareTo(last_year) > 0 || directory == String.Format($"BODACC/{DateTime.UtcNow.Year}"))
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
        }

        public void PopulateDB()
        {
            foreach (String subDirectory in Directory.GetDirectories(BODACC_DIR).OrderBy(d => d))
            {
                int year = int.Parse(new DirectoryInfo(subDirectory).Name);
                if (year > int.Parse(last_year) || year == DateTime.UtcNow.Year)
                {
                    PopulateDBYear(subDirectory, year);
                }
            }
            Console.WriteLine($"\n{TABLE_NAME} fully populated");
        }

        void DownloadYearlyData_Old(WebClient client, int year)
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

        void DownloadYearlyData_Recent(WebClient client, int year)
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

        void DecompressOld(String directory)
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

        void DecompressRecent(String directory)
        {
            var taz = Directory.GetFiles(directory, "PCL_BXA*");
            foreach (var file in taz.Where(f => f.EndsWith(".taz")))
            {
                Decompress(new FileInfo(file), "xzf");
            }
        }

        void PopulateDBYear(string directory, int year)
        {
            foreach (var file in Directory.GetFiles(directory, "*.xml").OrderBy(f => f))
            {
                DBProcessFile(file, year);
            }
        }

        static void Decompress(FileInfo fileToDecompress, string options)
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

        void DBProcessFile(String file, int year)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            XmlSerializer serializer = new XmlSerializer(typeof(PCL_REDIFF));

            using (XmlReader reader = XmlReader.Create(file))
            {
                PCL_REDIFF bulletin = (PCL_REDIFF)serializer.Deserialize(reader);
                parution = bulletin.Parution;
                if (State.Bodacc.LastParution.CompareTo(parution) <= 0)
                {
                    Commit(bulletin.Annonces.Annonce);
                    Console.Write($"\r{INSERT_COUNT} rows inserted");
                }
            }
        }


        string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        protected override string Transform(Annonce annonce)
        {
            var numeroAnnonce = annonce.NumeroAnnonce;
            if (State.Bodacc.LastNumero.CompareTo(numeroAnnonce) >= 0)
            {
                return "";
            }

            StringBuilder result = new StringBuilder();

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

            var rcs = annonce.NumeroImmatriculation.Any() ? annonce.NumeroImmatriculation.First().NumeroIdentificationRCS : "inconnu";
            var previousA = annonce.ParutionAvisPrecedent == null ? "-1" : annonce.ParutionAvisPrecedent.NumeroAnnonce;
            var previousP = annonce.ParutionAvisPrecedent == null ? "-1" : annonce.ParutionAvisPrecedent.NumeroParution;
            var type = annonce.TypeAnnonce.Creation != null ? "creation" :
                           (annonce.TypeAnnonce.Rectificatif != null ? "rectificatif" : "");
            DateTime date = default(DateTime);
            var french = CultureInfo.GetCultureInfo("fr-FR");
            var styles = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AllowTrailingWhite;
            if (annonce.Jugement != null && !String.IsNullOrWhiteSpace(annonce.Jugement.Date))
            {
                // TODO: fix encoding issues !!!
                var input = annonce.Jugement.Date
                    .Replace("1er", "1")
                    .Replace('\u00ef', ' ')
                    .Replace('\u00bf', ' ')
                    .Replace('\u00bd', ' ')
                    .Replace("f   evrier", "février")
                    .Replace("ao   t", "août")
                    .Replace("d   cembre", "décembre");
                DateTime parsed_date;
                if (!DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, styles, out parsed_date))
                {
                    if (!DateTime.TryParseExact(input, "d MMMM yyyy", french, styles, out parsed_date))
                    {
                        Console.WriteLine("cannot parse date : " + annonce.Jugement.Date);
                    }
                }

                date = parsed_date;
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

            // PARUTION,NUMERO,DATE, CODEPOSTAL,VILLE,NATURE,RCS,TYPE,FORMEJURIDIQUE, PREVIOUS_PARUTION,PREVIOUS_NUMERO
            result.Append(parution).Append(",");
            result.Append(numeroAnnonce).Append(",");
            result.Append(date).Append(",");
            result.Append(codePostal).Append(",");
            result.Append(ville).Append(",");
            result.Append(nature.ToLowerInvariant()).Append(",");
            result.Append(rcs.Replace(" ", "")
                    .Replace("\u00ef", "")
                    .Replace("\u00bf", "")
                    .Replace("\u00A0", "")
                    .Replace("\u00bd", "")).Append(",");
            result.Append(type.ToLowerInvariant()).Append(",");
            result.Append(forme.ToLowerInvariant()).Append(",");
            result.Append(previousP).Append(",");
            result.Append(previousA).Append(",");
            INSERT_COUNT += 1;
            return result.ToString();
        }
    }
}