using System;
using System.Collections.Generic;
using Npgsql;
using System.IO;
using System.Linq;

namespace bodacc
{
    // https://www.insee.fr/fr/information/2028273
    public class CategoriesJuridiques
    {
        static Dictionary<String, String> codes;
        static CategoriesJuridiques()
        {
            codes = new Dictionary<String, String>();
            foreach (var line in File.ReadAllLines(Path.Combine("INSEE", "categories_juridiques.csv")))
            {
                var split = line.Split(',');
                codes.Add(split[0], split[1]);
            }
        }

        public static void PopulateDB()
        {
            Console.WriteLine("populate categoriesjuridiques");
            if (Exists())
            {
                Console.WriteLine("categoriesjuridiques already populated -- aborting");
                return;
            }
            using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO categoriesjuridiques (CODE, LABEL)
                        VALUES (@Code, @Label)
                    ";
                    var codeParam = new NpgsqlParameter();
                    codeParam.ParameterName = "@Code";
                    command.Parameters.Add(codeParam);
                    var nomParam = new NpgsqlParameter();
                    nomParam.ParameterName = "@Label";
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
                    command.CommandText = "SELECT COUNT(*) FROM categoriesjuridiques";
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
    }
}