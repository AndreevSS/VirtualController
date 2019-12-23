using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace ru.pflb.VirtualController
{
    public class DBSender
    {
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
                Console.WriteLine(e.ToString());
            }
            Thread.Sleep(10);
        }
        public void StartSender(ConcurrentQueue<string> VCQueue)
        {
            //connection.Open();
            while (!isStopped)
            {

                
                while (!VCQueue.IsEmpty)
                {
                    if (VCQueue.TryDequeue(out string Result))
                    {

                        Console.WriteLine(DateTime.Now + ": VCQueue.count=" + VCQueue.Count + " Thread " + Thread.CurrentThread.Name + " Dequeuing '{0}'", Result);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(Result);
                        String sql = sb.ToString();
                        SqlCommand command = new SqlCommand(sql, this.connection);

                        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        SqlDataReader reader = command.ExecuteReader();
                        reader.Close();

                        Console.WriteLine("DB: "+ (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start) );
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                }
                Thread.Sleep(10);
            }
            connection.Close();
            Console.WriteLine(Thread.CurrentThread.Name + " stopped");
        }
    }
}
