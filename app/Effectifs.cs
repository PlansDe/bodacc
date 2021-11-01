using System;
using System.Collections.Generic;
using Npgsql;

namespace bodacc
{
    // https://www.insee.fr/fr/information/2028273
    public class Effectifs
    {
        public static void PopulateDB()
        {
            Console.WriteLine("populate effectifs");
            if (Exists())
            {
                Console.WriteLine("effectifs already populated -- aborting");
                return;
            }
            using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO effectifs (CODE, NOM)
                        VALUES (@Code, @Nom)
                    ";
                    var codeParam = new NpgsqlParameter();
                    codeParam.ParameterName = "@Code";
                    command.Parameters.Add(codeParam);
                    var nomParam = new NpgsqlParameter();
                    nomParam.ParameterName = "@Nom";
                    command.Parameters.Add(nomParam);

                    foreach (var kvp in codes)
                    {
                        codeParam.Value = kvp.Key;
                        nomParam.Value = kvp.Value;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
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
                    command.CommandText = "SELECT COUNT(*) FROM effectifs";
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

        private static Dictionary<String, String> codes = new Dictionary<string, string>
        {
            {"NN","Etablissement non employeur (pas de salarié au cours de l'année de référence et pas d'effectif au 31/12"},
            {"00","0 salarié (n'ayant pas d'effectif au 31/12 mais ayant employé des salariés au cours de l'année de référence"},
            {"01","1 ou 2 salariés"},
            {"02","3 à 5 salariés"},
            {"03","6 à 9 salariés"},
            {"11","10 à 19 salariés"},
            {"12","20 à 49 salariés"},
            {"21","50 à 99 salariés"},
            {"22","100 à 199 salariés"},
            {"31","200 à 249 salariés"},
            {"32","250 à 499 salariés"},
            {"41","500 à 999 salariés"},
            {"42","1 000 à 1 999 salariés"},
            {"51","2 000 à 4 999 salariés"},
            {"52","5 000 à 9 999 salariés"},
            {"53","10 000 salariés et plus" }
        };
    }
}