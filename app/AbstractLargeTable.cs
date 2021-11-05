using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using System.Diagnostics;

namespace bodacc
{
    public abstract class AbstractLargeTable<TRawData>
    {
        protected abstract String TABLE_NAME { get; }

        protected abstract String HEADER { get; }

        private int INSERT_COUNT { get; set; }

        protected void Commit(IEnumerable<TRawData> inputs)
        {
            List<String> csv = new List<string>();
            csv.Add(HEADER);
            foreach (var line in Transform(inputs))
            {
                INSERT_COUNT++;
                csv.Add(line);
            }

            var buff_path = Path.Combine("/usr/local/bin/tmp/", Path.GetRandomFileName());
            buff_path = Path.ChangeExtension(buff_path, ".csv");
            File.WriteAllLines(buff_path, csv);
            try
            {
                using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = String.Format($"COPY {TABLE_NAME}({HEADER}) FROM '{buff_path}' CSV HEADER");
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        Console.Write($"\r{INSERT_COUNT} rows inserted");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                File.Delete(buff_path);
            }
        }

        protected abstract IEnumerable<String> Transform(IEnumerable<TRawData> input);

        protected bool Exists()
        {
            using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = String.Format($"SELECT COUNT(*) FROM {TABLE_NAME}");
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

        protected void Delete()
        {
            using (var connection = new NpgsqlConnection(State.CONNECTION_STRING))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = String.Format($"DELETE FROM {TABLE_NAME}");
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
    }
}