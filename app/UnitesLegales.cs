using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

// TODO: merge that with Etablissements.cs in an abstract class
// TODO: psql is way slower than sqlite -- we should optimize somehow 
namespace bodacc
{
    // documentation : https://www.data.gouv.fr/fr/datasets/r/65946639-3150-4492-a988-944e2633531e
    public class SireneUnitesLegales : AbstractSireneTable
    {
        protected override String LOCAL_ARCHIVE => "stock-unites-legales.zip";
        protected override String LOCAL_FILENAME => "StockUniteLegale_utf8.csv";
        protected override String REMOTE_URL => "https://www.data.gouv.fr/fr/datasets/r/825f4199-cadd-486c-ac46-a65a8ea1a047";

        protected override string TABLE_NAME => "uniteslegales";

        protected override string HEADER => "SIREN,NOM,EFFECTIFS,ACTIVITE,CATEGORIEJURIDIQUE,NOMENCLATUREACTIVITE";

        public SireneUnitesLegales()
        {
        }

        protected override string Transform(string line)
        {
            StringBuilder result = new StringBuilder();
            var split = SireneCsvReader.ReadLine(line);
            if (split.Length != labels.Length)
            {
                Console.Error.WriteLine("cannot parse unitelegale : " + line);
                return "";
            }
            string siren = split[label_indices["siren"]];
            if (String.IsNullOrWhiteSpace(siren))
            {
                Console.WriteLine("unite legale sans siren -- ignored");
                return "";
            }


            var categorieJuridique = split[label_indices["categorieJuridiqueUniteLegale"]];
            var nom = split[label_indices["denominationUniteLegale"]];
            if (categorieJuridique == "1000" && String.IsNullOrWhiteSpace(nom))
            {
                nom = split[label_indices["prenom1UniteLegale"]] + " " + split[label_indices["nomUniteLegale"]];
            }

            // "SIREN,NOM,EFFECTIFS,ACTIVITE,CATEGORIEJURIDIQUE,NOMENCLATUREACTIVITE";
            result.Append(siren).Append(",");
            result.Append(nom).Append(",");
            result.Append(split[label_indices["trancheEffectifsUniteLegale"]]).Append(",");
            result.Append(split[label_indices["activitePrincipaleUniteLegale"]]).Append(",");
            result.Append(categorieJuridique).Append(",");
            result.Append(split[label_indices["nomenclatureActivitePrincipaleUniteLegale"]]);
            return result.ToString();
        }
    }
}