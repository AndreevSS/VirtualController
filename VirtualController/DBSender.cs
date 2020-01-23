using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace ru.pflb.VirtualController
{
    public class DBSender
    {
        Random rnd = new Random();
        public bool isStopped;
        SqlConnection connection;
        public DBSender(string[] DBData)
        {
            isStopped = false;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = DBData[0];
                builder.UserID = DBData[1];
                builder.Password = DBData[2];
                builder.InitialCatalog = DBData[3];

                connection = new SqlConnection(builder.ConnectionString);
                connection.Open();

         //       Console.WriteLine(Thread.CurrentThread.Name + " created");
            }
            catch (SqlException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                DisplaySqlErrors(e);
            }
            Thread.Sleep(1);
        }

        private static void DisplaySqlErrors(SqlException exception)
        {
            for (int i = 0; i < exception.Errors.Count; i++)
            {
                
                Console.WriteLine("Index #" + i + "\n" +
                    "Error: " + exception.Errors[i].ToString() + "\n");
            }
            Console.ReadLine();
        }

        public void StartSender(ConcurrentQueue<DBQueueObject> VCQueue)
        {
            //connection.Open();
            while (!isStopped)
            {

                
                while (!VCQueue.IsEmpty)
                {
                    if (VCQueue.TryDequeue(out DBQueueObject resultObject))
                    {
                        if (resultObject.needupdate)
                        {
                            resultObject.VR.dbready = false;
                        }


                        String Result = resultObject.Query;
                        Console.WriteLine(DateTime.Now + ": VCQueue.count=" + VCQueue.Count + " Thread " + Thread.CurrentThread.Name + " Dequeuing '{0}'", Result);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(Result);
                        String sql = sb.ToString();
                        SqlCommand command = new SqlCommand(sql, this.connection);

                        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        SqlDataReader reader = command.ExecuteReader();
                        reader.Close();

                        if (resultObject.needupdate)
                        {
                            resultObject.VR.dbready = true;
                        }


                        Console.WriteLine("DB: "+ (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start) );
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                }
                Thread.Sleep(1);
            }
            connection.Close();
            Console.WriteLine(Thread.CurrentThread.Name + " stopped");
        }
    }
}
