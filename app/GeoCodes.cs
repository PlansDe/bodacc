using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace bodacc
{
    // https://www.insee.fr/fr/information/2028273
    public class GeoCodes
    {
        const String DB_NAME = "bodacc.db";
        public static void PopulateDB()
        {
            Console.WriteLine("populate geocodes");
            if (Exists())
            {
                return;
            }

            using (var connection = new SqliteConnection(String.Format("Data Source={0}", DB_NAME)))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO geocodes (CODE, NOM)
                        VALUES (@Code, @Nom)
                    ";
                    var codeParam = new SqliteParameter();
                    codeParam.ParameterName = "@Code";
                    command.Parameters.Add(codeParam);
                    var nomParam = new SqliteParameter();
                    nomParam.ParameterName = "@Nom";
                    command.Parameters.Add(nomParam);

                    foreach (var kvp in codes)
                    {
                        codeParam.Value = int.Parse(kvp.Key);
                        nomParam.Value = kvp.Value;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
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
                    command.CommandText = "SELECT COUNT(*) FROM geocodes";
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int count = reader.GetInt32(0);
                                if (count == codes.Count)
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

        private static Dictionary<String, String> codes = new Dictionary<string, string> {
        {"99125","ALBANIE"},
        {"99109","ALLEMAGNE"},
        {"99130","ANDORRE"},
        {"99110","AUTRICHE"},
        {"99131","BELGIQUE"},
        {"99148","BIELORUSSIE"},
        {"99118","BOSNIE-HERZEGOVINE"},
        {"99111","BULGARIE"},
        {"99119","CROATIE"},
        {"99101","DANEMARK"},
        {"99134","ESPAGNE"},
        {"99106","ESTONIE"},
        {"99156","MACEDOINE DU NORD"},
        {"99105","FINLANDE"},
        {"99133","GIBRALTAR"},
        {"99126","GRECE"},
        {"99112","HONGRIE"},
        {"99136","IRLANDE, ou EIRE"},
        {"99102","ISLANDE"},
        {"99127","ITALIE"},
        {"99157","KOSOVO"},
        {"99107","LETTONIE"},
        {"99113","LIECHTENSTEIN"},
        {"99108","LITUANIE"},
        {"99137","LUXEMBOURG"},
        {"99144","MALTE"},
        {"99151","MOLDAVIE"},
        {"99138","MONACO"},
        {"99120","MONTENEGRO"},
        {"99103","NORVEGE"},
        {"99135","PAYS-BAS"},
        {"99122","POLOGNE"},
        {"99139","PORTUGAL"},
        {"99141","REPUBLIQUE DEMOCRATIQUE ALLEMANDE"},
        {"99142","REPUBLIQUE FEDERALE D'ALLEMAGNE"},
        {"99114","ROUMANIE"},
        {"99132","ROYAUME-UNI"},
        {"99123","RUSSIE"},
        {"99128","SAINT-MARIN"},
        {"99121","SERBIE"},
        {"99117","SLOVAQUIE"},
        {"99145","SLOVENIE"},
        {"99104","SUEDE"},
        {"99140","SUISSE"},
        {"99115","TCHECOSLOVAQUIE"},
        {"99116","TCHEQUIE"},
        {"99124","TURQUIE D'EUROPE"},
        {"99155","UKRAINE"},
        {"99129","VATICAN, ou SAINT-SIEGE"},
        {"99212","AFGHANISTAN"},
        {"99201","ARABIE SAOUDITE"},
        {"99252","ARMENIE"},
        {"99253","AZERBAIDJAN"},
        {"99249","BAHREIN"},
        {"99246","BANGLADESH"},
        {"99214","BHOUTAN"},
        {"99224","BIRMANIE"},
        {"99225","BRUNEI"},
        {"99234","CAMBODGE"},
        {"99216","CHINE"},
        {"99254","CHYPRE"},
        {"99237","COREE"},
        {"99239","COREE (REPUBLIQUE DE)"},
        {"99238","COREE (REPUBLIQUE POPULAIRE DEMOCRATIQUE DE)"},
        {"99247","EMIRATS ARABES UNIS"},
        {"99228","ETATS MALAIS NON FEDERES"},
        {"99255","GEORGIE"},
        {"99230","HONG-KONG"},
        {"99223","INDE"},
        {"99231","INDONESIE"},
        {"99204","IRAN"},
        {"99203","IRAQ"},
        {"99207","ISRAEL"},
        {"99217","JAPON"},
        {"99222","JORDANIE"},
        {"99211","KAMTCHATKA"},
        {"99256","KAZAKHSTAN"},
        {"99257","KIRGHIZISTAN"},
        {"99240","KOWEIT"},
        {"99241","LAOS"},
        {"99205","LIBAN"},
        {"99232","MACAO"},
        {"99227","MALAISIE"},
        {"99229","MALDIVES"},
        {"99218","MANDCHOURIE"},
        {"99242","MONGOLIE"},
        {"99215","NEPAL"},
        {"99250","OMAN"},
        {"99258","OUZBEKISTAN"},
        {"99213","PAKISTAN"},
        {"99261","PALESTINE (Etat de)"},
        {"99220","PHILIPPINES"},
        {"99221","POSSESSIONS BRITANNIQUES AU PROCHE-ORIENT"},
        {"99248","QATAR"},
        {"99209","SIBERIE"},
        {"99226","SINGAPOUR"},
        {"99235","SRI LANKA"},
        {"99206","SYRIE"},
        {"99259","TADJIKISTAN"},
        {"99236","TAIWAN"},
        {"99219","THAILANDE"},
        {"99262","TIMOR ORIENTAL"},
        {"99210","TURKESTAN RUSSE"},
        {"99260","TURKMENISTAN"},
        {"99208","TURQUIE"},
        {"99243","VIET NAM"},
        {"99244","VIET NAM DU NORD"},
        {"99245","VIET NAM DU SUD"},
        {"99251","YEMEN"},
        {"99233","YEMEN DEMOCRATIQUE"},
        {"99202","YEMEN (REPUBLIQUE ARABE DU)"},
        {"99319","ACORES, MADERE"},
        {"99303","AFRIQUE DU SUD"},
        {"99352","ALGERIE"},
        {"99395","ANGOLA"},
        {"99327","BENIN"},
        {"99347","BOTSWANA"},
        {"99331","BURKINA"},
        {"99321","BURUNDI"},
        {"99322","CAMEROUN"},
        {"99305","CAMEROUN ET TOGO"},
        {"99313","CANARIES (ILES)"},
        {"99396","CAP-VERT"},
        {"99323","CENTRAFRICAINE (REPUBLIQUE)"},
        {"99397","COMORES"},
        {"99324","CONGO"},
        {"99312","CONGO (REPUBLIQUE DEMOCRATIQUE)"},
        {"99326","COTE D'IVOIRE"},
        {"99399","DJIBOUTI"},
        {"99301","EGYPTE"},
        {"99317","ERYTHREE"},
        {"99391","ESWATINI"},
        {"99315","ETHIOPIE"},
        {"99328","GABON"},
        {"99304","GAMBIE"},
        {"99329","GHANA"},
        {"99330","GUINEE"},
        {"99314","GUINEE EQUATORIALE"},
        {"99392","GUINEE-BISSAU"},
        {"99320","ILES PORTUGAISES DE L'OCEAN INDIEN"},
        {"99332","KENYA"},
        {"99348","LESOTHO"},
        {"99302","LIBERIA"},
        {"99316","LIBYE"},
        {"99333","MADAGASCAR"},
        {"99334","MALAWI"},
        {"99335","MALI"},
        {"99350","MAROC"},
        {"99390","MAURICE"},
        {"99336","MAURITANIE"},
        {"99393","MOZAMBIQUE"},
        {"99311","NAMIBIE"},
        {"99337","NIGER"},
        {"99338","NIGERIA"},
        {"99339","OUGANDA"},
        {"99340","RWANDA"},
        {"99389","SAHARA OCCIDENTAL"},
        {"99306","SAINTE HELENE, ASCENSION ET TRISTAN DA CUNHA"},
        {"99394","SAO TOME-ET-PRINCIPE"},
        {"99341","SENEGAL"},
        {"99398","SEYCHELLES"},
        {"99342","SIERRA LEONE"},
        {"99318","SOMALIE"},
        {"99343","SOUDAN"},
        {"99307","SOUDAN ANGLO-EGYPTIEN, KENYA, OUGANDA"},
        {"99349","SOUDAN DU SUD"},
        {"99325","TANGER"},
        {"99309","TANZANIE"},
        {"99344","TCHAD"},
        {"99345","TOGO"},
        {"99351","TUNISIE"},
        {"99346","ZAMBIE"},
        {"99308","ZANZIBAR"},
        {"99310","ZIMBABWE"},
        {"99441","ANTIGUA-ET-BARBUDA"},
        {"99431","ANTILLES NEERLANDAISES"},
        {"99415","ARGENTINE"},
        {"99436","BAHAMAS"},
        {"99434","BARBADE"},
        {"99429","BELIZE"},
        {"99418","BOLIVIE"},
        {"99443","BONAIRE, SAINT EUSTACHE ET SABA"},
        {"99416","BRESIL"},
        {"99401","CANADA"},
        {"99417","CHILI"},
        {"99419","COLOMBIE"},
        {"99406","COSTA RICA"},
        {"99407","CUBA"},
        {"99444","CURAÇAO"},
        {"99408","DOMINICAINE (REPUBLIQUE)"},
        {"99438","DOMINIQUE"},
        {"99414","EL SALVADOR"},
        {"99420","EQUATEUR"},
        {"99404","ETATS-UNIS"},
        {"99435","GRENADE"},
        {"99430","GROENLAND"},
        {"99409","GUATEMALA"},
        {"99428","GUYANA"},
        {"99410","HAITI"},
        {"99411","HONDURAS"},
        {"99426","JAMAIQUE"},
        {"99403","LABRADOR"},
        {"99427","MALOUINES, OU FALKLAND (ILES)"},
        {"99405","MEXIQUE"},
        {"99412","NICARAGUA"},
        {"99413","PANAMA"},
        {"99421","PARAGUAY"},
        {"99422","PEROU"},
        {"99432","PORTO RICO"},
        {"99442","SAINT-CHRISTOPHE-ET-NIEVES"},
        {"99439","SAINTE-LUCIE	"},
        {"99445","SAINT-MARTIN (PARTIE NEERLANDAISE)"},
        {"99440","SAINT-VINCENT-ET-LES GRENADINES"},
        {"99437","SURINAME"},
        {"99402","TERRE-NEUVE"},
        {"99433","TRINITE-ET-TOBAGO"},
        {"99423","URUGUAY"},
        {"99424","VENEZUELA"},
        {"99425","VIERGES BRITANNIQUES (ILES)"},
        {"99501","AUSTRALIE"},
        {"99508","FIDJI"},
        {"99505","GUAM"},
        {"99504","HAWAII (ILES)"},
        {"99513","KIRIBATI"},
        {"99515","MARSHALL (ILES)"},
        {"99516","MICRONESIE (ETATS FEDERES DE)"},
        {"99507","NAURU"},
        {"99502","NOUVELLE-ZELANDE"},
        {"99517","PALAOS (ILES)"},
        {"99510","PAPOUASIE-NOUVELLE-GUINEE"},
        {"99503","PITCAIRN (ILE)"},
        {"99512","SALOMON (ILES)"},
        {"99506","SAMOA OCCIDENTALES"},
        {"99509","TONGA"},
        {"99511","TUVALU"},
        {"99514","VANUATU"}};
    }
}