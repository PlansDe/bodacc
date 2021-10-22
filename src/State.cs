using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace bodacc
{
    public class State
    {
        public static BodaccState Bodacc { get; set; }

        public class BodaccState
        {
            public int LastID { get; set; }
            public String LastParution { get; set; }
            public String LastNumero { get; set; }

            public BodaccState()
            {
                LastID = 1;
                LastParution = "";
                LastNumero = "";
            }
        }

        static State()
        {
            Bodacc = new BodaccState();
            if (File.Exists("bodacc.db"))
            {
                using (var connection = new SqliteConnection("Data Source=bodacc.db"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT ID,PARUTION,NUMERO from annonces 
                            ORDER BY PARUTION DESC, NUMERO DESC
                            LIMIT 1";
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Bodacc.LastID = int.Parse(reader.GetString(0));
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
}