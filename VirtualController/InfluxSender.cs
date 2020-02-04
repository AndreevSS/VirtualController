using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
//using InfluxDB.LineProtocol.Client;
//using InfluxDB.LineProtocol.Payload;
using InfluxDB.Net;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;

namespace ru.pflb.VirtualController
{
   public class InfluxSender
    {
        InfluxDb _client;
        string EmulName;
        string DBName = "RuntimeResources";
        public InfluxSender(string url, string EmulName)
        {
            this._client = new InfluxDb(url, "root", "root"); //"http://localhost:8086"
            this.EmulName = EmulName;
            Console.WriteLine("Influx connected");
        }

        public void SendQueueCount(int count)
        {

            Point point = new Point();
            point.Timestamp = DateTime.UtcNow;//.AddSeconds(-10);
            point.Measurement = "EmulQueues";

            point.Tags = new Dictionary<string, object>()
                {
                    {"EmulatorName", EmulName }
                };

            point.Precision = InfluxDB.Net.Enums.TimeUnit.Seconds;
            point.Fields = new Dictionary<string, object>()
            {

                {"DBQueueCount", count },
            };
            _client.WriteAsync(DBName, point);
        }
        public void SendResponseTime(long responsetime)
        {

            Point point = new Point();
            point.Timestamp = DateTime.UtcNow;//.AddSeconds(-10);
            point.Measurement = "EmulResponseTimes";

            point.Tags = new Dictionary<string, object>()
                {
                    {"EmulatorName", EmulName }
                };

            point.Precision = InfluxDB.Net.Enums.TimeUnit.Seconds;
            point.Fields = new Dictionary<string, object>()
            {

                {"Response_Time_MS", Convert.ToInt32(responsetime) },
            };
            _client.WriteAsync(DBName, point);
        }

        public void SendPing()
        {

            Point point = new Point();
            point.Timestamp = DateTime.UtcNow.AddSeconds(-10);
            point.Measurement = "EmulPings";

            point.Tags = new Dictionary<string, object>()
                {
                    {"EmulatorName", EmulName }
                };

            point.Precision = InfluxDB.Net.Enums.TimeUnit.Seconds;
            point.Fields = new Dictionary<string, object>()
            {
                {"Status", "OK" },
            };
            
                _client.WriteAsync(DBName, point);
            //    Thread.Sleep(10000);
            
        }
    }
}
