using System;
using System.IO;
using Npgsql;

namespace bodacc
{
    public class State
    {
        static string POSTGRE_HOST = Environment.GetEnvironmentVariable("POSTGRE_HOST");
        static string POPULATE_USER = Environment.GetEnvironmentVariable("POPULATE_USER");
        static string POPULATE_PASSWORD = Environment.GetEnvironmentVariable("POPULATE_PASSWORD");
        public static String CONNECTION_STRING =
            String.Format($"Server={POSTGRE_HOST};Port=5432;Database=bodacc;User ID={POPULATE_USER};Password={POPULATE_PASSWORD}");

        public static BodaccState Bodacc { get; set; }

        public class BodaccState
        {
            public String LastParution { get; set; }
            public String LastNumero { get; set; }

            public BodaccState()
            {
                LastParution = "2008-01-01";
                LastNumero = "0";
            }
        }

        static State()
        {
            Bodacc = new BodaccState();
            using (var connection = new NpgsqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                        SELECT PARUTION,NUMERO from annonces 
                            ORDER BY PARUTION DESC, NUMERO DESC
                            LIMIT 1";
                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Bodacc.LastParution = reader.GetString(1);
                            Bodacc.LastNumero = reader.GetString(2);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("cannot initialize bodacc state -- annonces table empty");
                }
            }
        }
    }
}