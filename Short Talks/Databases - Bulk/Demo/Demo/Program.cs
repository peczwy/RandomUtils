using Demo.Core;
using Demo.EF;
using Demo.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Demo
{
    public class Program
    {
        private static string Help = @"Hi, this is a simple program which shows how to use the bulk insert and merge operations with SQL Server.
Use one of the following options:
0 - to exit
1 - to create the data
2 - to load the bulk data... by row
3 - to load the bulk data
4 - to load the json data
5 - clear tracking table
6 - merge
? - to show help";

        private static string Merge = @"MERGE dbo.Simple AS TARGET
USING dbo.SimpleSnapshot AS SOURCE ON (TARGET.BusinessKey = SOURCE.BusinessKey AND TARGET.Active = 1)
WHEN MATCHED AND TARGET.Payload <> SOURCE.Payload THEN
	UPDATE SET 
		TARGET.Active = 0,
		TARGET.ValidTo = '{0}'
WHEN NOT MATCHED BY TARGET THEN
	INSERT ([BusinessKey] ,[Payload] ,[ValidFrom] ,[ValidTo],[Active]) 
	VALUES(SOURCE.BusinessKey, SOURCE.Payload, '{1}', NULL, 1);";

        private static Random Random = new Random(1337);

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private static string Serialize<T>(T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = true;
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriterWithEncoding(new StringBuilder(), Encoding.UTF8);
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    xmlserializer.Serialize(writer, value, new XmlSerializerNamespaces());
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        private static void option1()
        {
            var list = new List<SimpleSnapshotTable>();
            Console.WriteLine("How many records:");
            if (int.TryParse(Console.ReadLine(), out int count))
            {
                for (int i = 0; i < count; ++i)
                {
                    list.Add(new SimpleSnapshotTable() { BusinessKey = i, Payload = RandomString(Random.Next(1, 100)) });
                }
                using (var file = new StreamWriter(@"Bulk.txt"))
                {
                    foreach (var entity in list)
                    {
                        file.WriteLine(string.Format("{0};{1}", entity.BusinessKey, entity.Payload));
                    }
                }
                using (var file = new StreamWriter(@"Xmled.xml"))
                {
                    file.WriteLine(Serialize(list));
                }
            }
        }

        private static void option2()
        {
            Console.WriteLine("How many records per batch:");
            if (int.TryParse(Console.ReadLine(), out int batch))
            {
                if (batch > 0)
                {
                    using (var context = new SimpleContext())
                    {
                        context.Database.ExecuteSqlCommand("DELETE FROM dbo.SimpleSnapshot");
                        context.Configuration.AutoDetectChangesEnabled = false;
                        context.Configuration.ValidateOnSaveEnabled = false;
                        var start = DateTime.Now;
                        var count = 0;
                        foreach (var line in File.ReadAllLines("Bulk.txt"))
                        {
                            var split = line.Split(';');
                            count++;
                            context.SimpleSnapshot.Add(new SimpleSnapshotTable() { BusinessKey = int.Parse(split[0]), Payload = split[1] });
                            if (count % batch == 0) { context.SaveChanges(); }
                        }
                        context.SaveChanges();
                        var end = DateTime.Now;
                        Console.WriteLine("Writing of {0} records finished in {1} seconds", count, (end - start).TotalSeconds);
                    }
                }
            }
        }

        private static void option3()
        {
            var cs = ConfigurationManager.ConnectionStrings["Simple"].ToString();
            using (var connection = new SqlConnection(cs))
            {
                using (var context = new SimpleContext())
                {
                    context.Database.ExecuteSqlCommand("DELETE FROM dbo.SimpleSnapshot");
                    var transaction = (SqlTransaction)null;
                    connection.Open();
                    try
                    {
                        var start = DateTime.Now;

                        var dt = new DataTable("SimpleSnapshot");
                        dt.Columns.Add("BusinessKey", typeof(int));
                        dt.Columns.Add("Payload", typeof(string));

                        int count = 0;
                        foreach (var line in File.ReadAllLines("Bulk.txt"))
                        {
                            var split = line.Split(';');
                            dt.Rows.Add(split[0], split[1]);
                            count++;
                        }

                        transaction = connection.BeginTransaction();
                        using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
                        {
                            sqlBulkCopy.DestinationTableName = "SimpleSnapshot";
                            sqlBulkCopy.ColumnMappings.Add("BusinessKey", "BusinessKey");
                            sqlBulkCopy.ColumnMappings.Add("Payload", "Payload");

                            sqlBulkCopy.WriteToServer(dt);
                        }
                        transaction.Commit();

                        var end = DateTime.Now;
                        Console.WriteLine("Batch finished in {0} seconds", (end - start).TotalSeconds);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        private static void option4()
        {
            var cs = ConfigurationManager.ConnectionStrings["Simple"].ToString();
            using (var connection = new SqlConnection(cs))
            {
                using (var context = new SimpleContext())
                {
                    context.Database.ExecuteSqlCommand("DELETE FROM dbo.SimpleSnapshot");
                    var transaction = (SqlTransaction)null;
                    connection.Open();
                    try
                    {
                        var start = DateTime.Now;
                        var dt = new DataTable("SimpleSnapshot");
                        dt.Columns.Add("BusinessKey", typeof(int));
                        dt.Columns.Add("Payload", typeof(string));

                        var doc = XDocument.Load(@"Xmled.xml");
                        int count = 0;
                        foreach (var rowXml in doc.Descendants().Where(x => x.Name.LocalName.Equals("SimpleSnapshotTable")))
                        {
                            var bk = rowXml.Descendants().Where(x => x.Name.LocalName.Equals("BusinessKey")).First().Value;
                            var pl = rowXml.Descendants().Where(x => x.Name.LocalName.Equals("Payload")).First().Value;
                            dt.Rows.Add(bk, pl);
                            count++;
                        }

                        transaction = connection.BeginTransaction();
                        using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
                        {
                            sqlBulkCopy.DestinationTableName = "SimpleSnapshot";
                            sqlBulkCopy.ColumnMappings.Add("BusinessKey", "BusinessKey");
                            sqlBulkCopy.ColumnMappings.Add("Payload", "Payload");

                            sqlBulkCopy.WriteToServer(dt);
                        }
                        transaction.Commit();

                        var end = DateTime.Now;
                        Console.WriteLine("Batch finished in {0} seconds", (end - start).TotalSeconds);
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        private static void option5()
        {
            using (var context = new SimpleContext())
            {
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.Simple;");
            }
        }

        private static void option6()
        {
            using (var context = new SimpleContext())
            {
                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var sql = string.Format(Merge, date, date);
                context.Database.CommandTimeout = 3000;
                context.Database.ExecuteSqlCommand(sql);
                context.Database.ExecuteSqlCommand(sql);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Help);

            var input = (string)null;
            using (var context = new SimpleContext()) { var x = context.Simple.ToList(); };
            do
            {
                Console.WriteLine("Choose option:");
                input = Console.ReadLine();
                switch (input)
                {
                    case "0": return;
                    case "1": option1(); break;
                    case "2": option2(); break; 
                    case "3": option3(); break; 
                    case "4": option4(); break;
                    case "5": option5(); break;
                    case "6": option6(); break;
                    default: Console.WriteLine(Help); break;
                }
            } while (!"0".Equals(input));
            
        }
    }
}
