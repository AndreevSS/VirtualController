using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace ru.pflb.VirtualController
{
    class DBSender
    {
        SqlConnection connection;
        public DBSender(string DataSource, string UserID, string Password, string InitialCatalog)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = DataSource;
                builder.UserID = UserID;
                builder.Password = Password;
                builder.InitialCatalog = InitialCatalog;

                connection = new SqlConnection(builder.ConnectionString);
                connection.Open();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            Thread.Sleep(50);
        }
        public void StartSender(ConcurrentQueue<string> VCQueue)
        {
            //connection.Open();
            while (true)
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
                        SqlDataReader reader = command.ExecuteReader();
                        reader.Close();
                       
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                }
                Thread.Sleep(50);
            }
            connection.Close();
        }
    }
}
