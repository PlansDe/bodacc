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
    public class SireneImport
    {
        const String DB_NAME = "bodacc.db";
        const String SIRENE_DIR = "SIRENE";
        const String LOCAL_ARCHIVE = "stock-etablissements.zip";
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
                System.IO.File.SetLastWriteTimeUtc(local_archive.FullName, DateTime.UtcNow);
            }
        }

        public static void Decompress(bool forceUpdate = false)
        {
            var local_csv = new FileInfo(Path.Combine(SIRENE_DIR, LOCAL_FILENAME));
            if (forceUpdate || !local_csv.Exists || DateTime.UtcNow - local_csv.LastWriteTimeUtc > TimeSpan.FromDays(1))
            {
                ZipFile.ExtractToDirectory(Path.Combine(SIRENE_DIR, LOCAL_ARCHIVE), SIRENE_DIR);
                System.IO.File.SetLastWriteTimeUtc(local_csv.FullName, DateTime.UtcNow);
            }
        }

        public static void PopulateDb()
        {
            using (var fileStream = File.OpenRead(Path.Combine(SIRENE_DIR, LOCAL_FILENAME)))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096))
            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DB_NAME)))
            {
                connection.Open();
                String head = streamReader.ReadLine();
                int keyIndex = 0;
                var labels = head.Split(",");
                var labels_indices = labels.ToDictionary(s => s, s => keyIndex++);
                int ID = 1;
                List<String> lines = new List<string>();
                while (ReadLines(streamReader, 100000, lines) != 0)
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = @"
                        INSERT INTO etablissements (ID, SIREN, SIRET, ETATADMIN,EFFECTIFS,CP,CP2,VILLE1,VILLE2,NOM,PAYS1,PAYS2,VILLEETRANGER1,VILLEETRANGER2,ACTIVITE)
                        VALUES (@ID,@Siren,@Siret,@EtatAdmin,@Effectifs,@CP,@CP2,@Ville,@Ville2,@Nom,@Pays1,@Pays2,@VilleEtranger1,@VilleEtranger2,@Activite)
                    ";
                        var pId = new SqliteParameter();
                        pId.ParameterName = "@ID";
                        command.Parameters.Add(pId);
                        var pSiren = new SqliteParameter();
                        pSiren.ParameterName = "@Siren";
                        command.Parameters.Add(pSiren);
                        var pSiret = new SqliteParameter();
                        pSiret.ParameterName = "@Siret";
                        command.Parameters.Add(pSiret);
                        var ptatAdmin = new SqliteParameter();
                        ptatAdmin.ParameterName = "@EtatAdmin";
                        command.Parameters.Add(ptatAdmin);
                        var pEff = new SqliteParameter();
                        pEff.ParameterName = "@Effectifs";
                        command.Parameters.Add(pEff);
                        var pCp = new SqliteParameter();
                        pCp.ParameterName = "@CP";
                        command.Parameters.Add(pCp);
                        var pCp2 = new SqliteParameter();
                        pCp2.ParameterName = "@CP2";
                        command.Parameters.Add(pCp2);
                        var pVille = new SqliteParameter();
                        pVille.ParameterName = "@Ville";
                        command.Parameters.Add(pVille);
                        var pVille2 = new SqliteParameter();
                        pVille2.ParameterName = "@Ville2";
                        command.Parameters.Add(pVille2);
                        var pNom = new SqliteParameter();
                        pNom.ParameterName = "@Nom";
                        command.Parameters.Add(pNom);
                        var pPays1 = new SqliteParameter();
                        pPays1.ParameterName = "@Pays1";
                        command.Parameters.Add(pPays1);
                        var pPays2 = new SqliteParameter();
                        pPays2.ParameterName = "@Pays2";
                        command.Parameters.Add(pPays2);
                        var pVilleE = new SqliteParameter();
                        pVilleE.ParameterName = "@VilleEtranger1";
                        command.Parameters.Add(pVilleE);
                        var pVilleE2 = new SqliteParameter();
                        pVilleE2.ParameterName = "@VilleEtranger2";
                        command.Parameters.Add(pVilleE2);
                        var pActivite = new SqliteParameter();
                        pActivite.ParameterName = "@Activite";
                        command.Parameters.Add(pActivite);
                        foreach (string line in lines)
                        {
                            var split = line.Split(",");
                            pId.Value = ID++;
                            pSiren.Value = split[labels_indices["siren"]];
                            pSiret.Value = split[labels_indices["siret"]];
                            ptatAdmin.Value = split[labels_indices["etatAdministratifEtablissement"]];
                            pEff.Value = split[labels_indices["trancheEffectifsEtablissement"]];
                            pCp.Value = split[labels_indices["codePostalEtablissement"]];
                            pCp2.Value = split[labels_indices["codePostal2Etablissement"]];
                            pVille.Value = split[labels_indices["libelleCommuneEtablissement"]];
                            pVille2.Value = split[labels_indices["libelleCommune2Etablissement"]];
                            pNom.Value = split[labels_indices["denominationUsuelleEtablissement"]];
                            pPays1.Value = split[labels_indices["codePaysEtrangerEtablissement"]];
                            pPays2.Value = split[labels_indices["codePaysEtranger2Etablissement"]];
                            pVilleE.Value = split[labels_indices["libelleCommuneEtrangerEtablissement"]];
                            pVilleE2.Value = split[labels_indices["libelleCommuneEtranger2Etablissement"]];
                            pActivite.Value = split[labels_indices["activitePrincipaleEtablissement"]];
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.Write("\r{0} entries processed", ID - 1);
                    }
                }
            }

            static int ReadLines(StreamReader streamReader, int count, List<String> lines)
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
}