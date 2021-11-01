using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace bodacc
{
    // documentation : https://www.data.gouv.fr/fr/datasets/r/65946639-3150-4492-a988-944e2633531e
    public class SireneEtablissements
    {
        const String SIRENE_DIR = "SIRENE";
        const String LOCAL_ARCHIVE = "stock-etablissements.zip";
        const String LOCAL_FILENAME = "StockEtablissement_utf8.csv";
        const String REMOTE_URL = "https://www.data.gouv.fr/fr/datasets/r/0651fb76-bcf3-4f6a-a38d-bc04fa708576";

        public static void DownloadData(bool forceUpdate = false)
        {
            Console.WriteLine("download etablissements from SIRENE");
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

        public static bool Decompress(bool forceUpdate = false)
        {
            Console.WriteLine("decompress etablissements");
            var local_csv = new FileInfo(Path.Combine(SIRENE_DIR, LOCAL_FILENAME));
            if (forceUpdate || !local_csv.Exists || DateTime.UtcNow - local_csv.LastWriteTimeUtc > TimeSpan.FromDays(7))
            {
                if (File.Exists(local_csv.FullName))
                {
                    File.Delete(local_csv.FullName);
                }
                ZipFile.ExtractToDirectory(Path.Combine(SIRENE_DIR, LOCAL_ARCHIVE), SIRENE_DIR);
                System.IO.File.SetLastWriteTimeUtc(local_csv.FullName, DateTime.UtcNow);
                return true;
            }

            return false;
        }

        public static void PopulateDb(bool forceUpdate = false)
        {
            Console.WriteLine("populate etablissements");
            if (Exists())
            {
                if (forceUpdate)
                {
                    using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            var command = connection.CreateCommand();
                            command.CommandText = "DELETE FROM uniteslegales";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("etablissements already exists -- aborting");
                    return;
                }
            }

            using (var fileStream = File.OpenRead(Path.Combine(SIRENE_DIR, LOCAL_FILENAME)))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
            using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
            {
                connection.Open();
                String head = streamReader.ReadLine();
                int keyIndex = 0;
                var labels = head.Split(",");
                var labels_indices = labels.ToDictionary(s => s, s => keyIndex++);
                int ID = 1;
                List<String> lines = new List<string>();
                while (streamReader.ReadLines(100000, lines) != 0)
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = @"
                        INSERT INTO etablissements (SIRET, SIREN, ETATADMIN,EFFECTIFS,CP,VILLE,NOM,PAYS,VILLEETRANGER,ACTIVITE, NOMENCLATUREACTIVITE)
                        VALUES (@Siret,@Siren,@EtatAdmin,@Effectifs,@CP,@Ville,@Nom,@Pays1,@VilleEtranger1,@Activite, @NomAct)
                    ";
                        var pSiret = new NpgsqlParameter();
                        pSiret.ParameterName = "@Siret";
                        command.Parameters.Add(pSiret);
                        var pSiren = new NpgsqlParameter();
                        pSiren.ParameterName = "@Siren";
                        command.Parameters.Add(pSiren);
                        var ptatAdmin = new NpgsqlParameter();
                        ptatAdmin.ParameterName = "@EtatAdmin";
                        command.Parameters.Add(ptatAdmin);
                        var pEff = new NpgsqlParameter();
                        pEff.ParameterName = "@Effectifs";
                        command.Parameters.Add(pEff);
                        var pCp = new NpgsqlParameter();
                        pCp.ParameterName = "@CP";
                        command.Parameters.Add(pCp);
                        var pVille = new NpgsqlParameter();
                        pVille.ParameterName = "@Ville";
                        command.Parameters.Add(pVille);
                        var pNom = new NpgsqlParameter();
                        pNom.ParameterName = "@Nom";
                        command.Parameters.Add(pNom);
                        var pPays1 = new NpgsqlParameter();
                        pPays1.ParameterName = "@Pays1";
                        command.Parameters.Add(pPays1);
                        var pVilleE = new NpgsqlParameter();
                        pVilleE.ParameterName = "@VilleEtranger1";
                        command.Parameters.Add(pVilleE);
                        var pActivite = new NpgsqlParameter();
                        pActivite.ParameterName = "@Activite";
                        command.Parameters.Add(pActivite);
                        var pNomAct = new NpgsqlParameter();
                        pNomAct.ParameterName = "@NomAct";
                        command.Parameters.Add(pNomAct);
                        foreach (string line in lines)
                        {
                            var split = SireneCsvReader.ReadLine(line);
                            if (split.Length != labels.Length)
                            {
                                Console.Error.WriteLine("cannot parse etablissement : " + line);
                                continue;
                            }
                            var siret = split[labels_indices["siret"]];
                            if (String.IsNullOrEmpty(siret))
                            {
                                Console.WriteLine("etablissement sans siret -- ignored");
                                continue;
                            }
                            var siren = split[labels_indices["siren"]];
                            if (String.IsNullOrEmpty(siren))
                            {
                                Console.WriteLine("etablissement sans siren -- ignored");
                                continue;
                            }
                            pSiren.Value = siren.Replace(" ", "");
                            pSiret.Value = siret.Replace(" ", "");
                            ptatAdmin.Value = split[labels_indices["etatAdministratifEtablissement"]];
                            pEff.Value = split[labels_indices["trancheEffectifsEtablissement"]];
                            pCp.Value = split[labels_indices["codePostalEtablissement"]];
                            pVille.Value = split[labels_indices["libelleCommuneEtablissement"]];
                            var nom = split[labels_indices["denominationUsuelleEtablissement"]];
                            if (String.IsNullOrEmpty(nom))
                            {
                                nom = split[labels_indices["enseigne1Etablissement"]];
                                if (!String.IsNullOrEmpty(nom))
                                {
                                    nom = nom + "...";
                                }
                            }
                            pNom.Value = nom;
                            pPays1.Value = split[labels_indices["codePaysEtrangerEtablissement"]];
                            pVilleE.Value = split[labels_indices["libelleCommuneEtrangerEtablissement"]];
                            pActivite.Value = split[labels_indices["activitePrincipaleEtablissement"]];
                            pNomAct.Value = split[labels_indices["nomenclatureActivitePrincipaleEtablissement"]];

                            ID++;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.Write("\r{0} entries processed", ID - 1);
                    }
                }
            }
        }



        private static bool Exists()
        {
            using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM etablissements";
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