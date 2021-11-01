using System;
using System.IO;
using Npgsql;

namespace bodacc
{
    public class State
    {
        // TODO: hide password in secret
        public const String CONNECTION_STRING = "Server=172.22.0.2;Port=5432;Database=bodacc;User ID=populate;Password=88e359f4f79166de265d2a403e38e7d5";

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