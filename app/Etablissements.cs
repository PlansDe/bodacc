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
    public class SireneEtablissements : AbstractSireneTable
    {
        protected override String LOCAL_ARCHIVE => "stock-etablissements.zip";
        protected override String LOCAL_FILENAME => "StockEtablissement_utf8.csv";
        protected override String REMOTE_URL => "https://www.data.gouv.fr/fr/datasets/r/0651fb76-bcf3-4f6a-a38d-bc04fa708576";

        public SireneEtablissements()
        {
            INSERT_COUNT = 0;
        }

        protected override string TABLE_NAME => "etablissements";

        protected override string HEADER => "SIRET, SIREN, ETATADMIN,EFFECTIFS,CP,VILLE,NOM,PAYS,VILLEETRANGER,ACTIVITE, NOMENCLATUREACTIVITE";

        protected override string Transform(string line)
        {
            var split = SireneCsvReader.ReadLine(line);
            if (split.Length != labels.Length)
            {
                Console.Error.WriteLine("cannot parse etablissement : " + line);
                return "";
            }
            var siret = split[label_indices["siret"]];
            if (String.IsNullOrEmpty(siret))
            {
                Console.WriteLine("etablissement sans siret -- ignored");
                return "";
            }
            var siren = split[label_indices["siren"]];
            if (String.IsNullOrEmpty(siren))
            {
                Console.WriteLine("etablissement sans siren -- ignored");
                return "";
            }

            // (@Siret,@Siren,@EtatAdmin,@Effectifs,@CP,@Ville,@Nom,@Pays1,@VilleEtranger1,@Activite, @NomAct)
            StringBuilder result = new StringBuilder();
            result.Append(siret.Replace(" ", "")).Append(",");
            result.Append(siren.Replace(" ", "")).Append(",");
            result.Append(split[label_indices["etatAdministratifEtablissement"]]).Append(",");
            result.Append(split[label_indices["trancheEffectifsEtablissement"]]).Append(",");
            result.Append(split[label_indices["codePostalEtablissement"]]).Append(",");
            result.Append(split[label_indices["libelleCommuneEtablissement"]]).Append(",");
            var nom = split[label_indices["denominationUsuelleEtablissement"]];
            if (String.IsNullOrEmpty(nom))
            {
                nom = split[label_indices["enseigne1Etablissement"]];
                if (!String.IsNullOrEmpty(nom))
                {
                    nom = nom + "...";
                }
            }
            result.Append(nom).Append(",");
            result.Append(split[label_indices["codePaysEtrangerEtablissement"]]).Append(",");
            result.Append(split[label_indices["libelleCommuneEtrangerEtablissement"]]).Append(",");
            result.Append(split[label_indices["activitePrincipaleEtablissement"]]).Append(",");
            result.Append(split[label_indices["nomenclatureActivitePrincipaleEtablissement"]]);
            return result.ToString();
        }
    }
}