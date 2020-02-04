using CSharpTest.Net.Http;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace ru.pflb.VirtualController
{
    public class VirtualRobot
    {  

        public ConcurrentQueue<DBQueueObject> VCQueue;
//        public DBQueueObject DBQueueObject;
        public bool dbready;

        public string BPAPrefix;

        string bpaqueuename;
        public int port;
        public string id;
        public string token;
        public VirtualSession VS;
        public HttpServer server;
        int SessionDuration = 0;
        bool UseBPAQueue;

        public HttpListener listener = new HttpListener();
        Random rnd = new Random();
        string userid = "";
        string processid = "";
        string tags = "";

        InfluxSender influxSender;

        public VirtualRobot(int port, ConcurrentQueue<DBQueueObject> VCQueue, HttpServer server, string BPAPrefix, InfluxSender influxSender)
        {
            this.port = port;
            this.id = Guid.NewGuid().ToString();
            this.token = null;
            this.VCQueue = VCQueue;
            this.server = server;
            this.UseBPAQueue = false;
            this.dbready = false;

            this.influxSender = influxSender;
            this.BPAPrefix = BPAPrefix;

            DBQueueObject DBQueueObject = new DBQueueObject(this);
            //DBQueueObject.needupdate = true;
            DBQueueObject.Query = DBQueries.UpdateBPAResource(port, 2, 0, BPAPrefix);
            VCQueue.Enqueue(DBQueueObject);
            //  this.DBQueueObject = new DBQueueObject(this);

            //            this.DBQueueObject.needupdate = true;

            //            VCQueue.Enqueue(DBQueries.CreateBPAResource(id, port));


            Console.WriteLine("VirtualRobot created on " + port);
        }


        public void RobotRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                // Construct a response.
                NameValueCollection BodyCol = new NameValueCollection();
                BodyCol = ConnectionHandler.KeysAndValuesFromBody(request.InputStream);
                String Path = context.Request.RawUrl;

                String[] SplitPath = Path.Split('&');

                string pattern = "\\b(.+?)(?(?=(%20.*))(.*)|$)";

                string responseText = "";

                long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                String RequestTime = DateTime.Now.ToString();
                Console.WriteLine(RequestTime + " Request: \n" + context.Request.Url);//+ "\n Response: \n" + responseText);


                if (request.LocalEndPoint.Port == port)
                {
                    SessionDuration = 0;

                    foreach (String command in SplitPath)
                    {
                        bool isValidCommand = true;
                        GroupCollection pathGroup = Regex.Match(command, pattern).Groups;
                        GroupCollection groups;

                        //       for (int i = 0; i < pathGroup.Count; i++)
                        //       {
                        //           Console.WriteLine("\npathGroup[" + i + "] = " + pathGroup[i].Value);
                        //       }

                        switch (pathGroup[1].Value)
                        {
                            default: isValidCommand = false; break;
                            case "user":
                                break;
                            case "password":
                                break;
                            case "busy":
                                break;
                            case "getauthtoken":
                                groups = Regex.Match(pathGroup[2].Value, "%20(.*?)%20(.*?)%20(.*)").Groups;
                                this.processid = groups[1].Value;
                                userid = groups[2].Value;
                                responseText += CreateToken(VCQueue, userid);
                                break;
                            case "createas":
                                responseText += CreateSession(VCQueue, userid);
                                break;
                            case "startp":
                                Console.WriteLine("pathGroup[2].Value = " + pathGroup[2].Value);

                                groups = Regex.Match(pathGroup[2].Value, "input%20name=\'Wait%20\\(sec\\)\'%20type=\'number\'%20value=\'(.*?)\'").Groups;

                                dbready = false;

                                if (!(groups[1].Value is null))
                                {
                                    SessionDuration = Convert.ToInt32(groups[1].Value);
                                }
                                else
                                {
                                    SessionDuration = 0;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("!!!Wait (sec) not found!!!");
                                }

                                groups = Regex.Match(pathGroup[2].Value, "input%20name=\'Scenario'%20type=\'text\'%20value=\'(.*?)\'").Groups;

                                for (int i = 0; i < groups.Count; i++)
                                {
                                    Console.WriteLine(groups[i].Value);
                                }
                                if ((groups[1].Value == "F"))
                                {
                                    UseBPAQueue = false;
                                }
                                else
                                {
                                    UseBPAQueue = true;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("UseBPAQueue = true\n Scenario = " + groups[1].Value);
                                }

                                groups = Regex.Match(pathGroup[2].Value, "input%20name=\'Queue'%20type=\'text\'%20value=\'(.*?)\'").Groups;



                                if (!(groups[1].Value is null))
                                {
                                    bpaqueuename = groups[1].Value;
                                }
                                else
                                {
                                    bpaqueuename = "";
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("!!!BPAQueue is not found!!!");
                                }

                                groups = Regex.Match(pathGroup[2].Value, "input%20name=\'Tag'%20type=\'text\'%20value=\'(.*?)\'").Groups;

                                tags = "";

                                if (!(groups[1].Value is null))
                                {
                                    tags = groups[1].Value;
                                   
                                }
                                else
                                {
                                    tags = "";
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("!!!Tag is not found!!!");
                                }


                                //                                < inputs ><input name='Wait (sec)' type='number' value='60'/><input name='Scenario' type='text' value='F'/></inputs>

                                responseText += StartSession(VCQueue, SessionDuration, UseBPAQueue);
                                break;
                            case "startas":
                                //             responseText += StartSession(VCQueue, SessionDuration);
                                break;
                            case "deleteas":
                                groups = Regex.Match(pathGroup[2].Value, "%20(.*?)%20(.*?)%20(.*)").Groups;
                                string sessionid = groups[2].Value;
                                responseText += DeleteSession(VCQueue, sessionid);

                                dbready = true;
                                break;
                        }
                        if (!(isValidCommand))
                        {
                            dbready = true;
                            responseText = "400\r\ncommand '" + pathGroup[1] + "' not found";
                            context.Response.StatusCode = 400;
                            break;
                        }

                    }
                    
                    while (dbready != true)
                    {
                        Thread.Sleep(10);
                    }

                    Console.WriteLine("db ready");

                /*    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(RequestTime + " Request: \n" + context.Request.Url + "\n" + DateTime.Now + " Response: \n" + responseText);
                    //   ConnectionHandler.PrintKeysAndValues(BodyCol);
                    Console.ForegroundColor = ConsoleColor.Gray;
                */    Thread.Sleep(3);


                    ConnectionHandler.SimpleTextResponse(context, responseText);
                    long ResponseTimeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start;
                    Console.ForegroundColor = ConsoleColor.White;
                    String logline = RequestTime + "\n ResponseTime(ms): " + ResponseTimeMS + "\n" +
                        " Request: \n" + context.Request.Url + "\n" + DateTime.Now + " Response: \n" + responseText;
                    Console.WriteLine(logline);

                    influxSender.SendResponseTime(ResponseTimeMS);

                    if (ResponseTimeMS > 10000)
                    {
                        Console.WriteLine("!!!!!!!!!!!!!!!! Responsetime > 10000 !!!!!!!!!!!!!!!!!");
                        /*              try
                                      {
                                          string path = @"log\Robot" + port + "_LONG_responseTimes.log";
                                          using (StreamWriter sw = File.AppendText(path))
                                          {

                                              sw.WriteLine(logline);

                                          }
                                          //   System.IO.File.WriteAllText(@"log\hlo.log", "5r5");
                                      }
                                      catch (Exception e)
                                      {
                                          Console.WriteLine(e);
                                      }

                                  }

                                  /*try
                                  {
                                      string path = @"log\Robot" + port + "_responseTimes.log";
                                      using (StreamWriter sw = File.AppendText(path))
                                      {

                                          sw.WriteLine(logline);

                                      }
                                      //   System.IO.File.WriteAllText(@"log\hlo.log", "5r5");
                                  }
                                  catch (Exception e)
                                  {
                                      Console.WriteLine(e);
                                  }
              */
                    }
                    //   ConnectionHandler.PrintKeysAndValues(BodyCol);
                    Console.ForegroundColor = ConsoleColor.Gray;




                    dbready = false;
                    Thread.Sleep(1);
                }

            }

            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("error: " + e);
            }
        }

        string CreateToken(ConcurrentQueue<DBQueueObject> VCQueue, String UserID)
        {

            token = Guid.NewGuid().ToString();

            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            DBQueueObject DBQueueObject = new DBQueueObject(this);

            DBQueueObject.Query = DBQueries.CreateToken(userid, token);
            DBQueueObject.needupdate = true;
            VCQueue.Enqueue(DBQueueObject);


            String Result = UserID + " " + token;
            //было String Result = UserID + "" + token;

            int ResponseTokenLength = 73;
            for (int i = 70; i < ResponseTokenLength; i++)
            {
                if (Result.Length < ResponseTokenLength)
                {
                    Result = Result + '0';
                }
            }

            return Result;
        }
        string CreateSession(ConcurrentQueue<DBQueueObject> VCQueue, string userid)
        {
            String sessionid = Guid.NewGuid().ToString();
            // Thread.Sleep(rnd.Next(50));
            VS = new VirtualSession(sessionid, 0, port, this);

            if (VS is null)
            {
                Console.WriteLine("\t\t\tCreatedSession::VS is null\n");
            }

            //VCQueue.Enqueue(DBQueries.CreateSession(sessionid, processid, userid, port));

            DBQueueObject DBQueueObject = new DBQueueObject(this);


            DBQueueObject.Query = DBQueries.CreateSession(sessionid, processid, userid, port, BPAPrefix);

            DBQueueObject.needupdate = true;

            
            VCQueue.Enqueue(DBQueueObject);

            if (VS is null)
            {
                Console.WriteLine("\t\t\tCreatedSession::VS is null\n");
            }

            return
                   "USER SET\r\n" +
                   "USER AUTHENTICATED\r\n" +
                   "no\r\n" +
                   "SESSION CREATED : " + sessionid;

        }
        string StartSession(ConcurrentQueue<DBQueueObject> VCQueue, int duration, bool UseBPAQueue)
        {
            if (VS is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n!!!!!!!!!!VS is null!!!!!!!!!!!!!!\n\n");
            }
            else
            {
                Console.WriteLine("\n\n!!!!!!!!!!!!VS.status = " + VS.status + "!!!!!!!!!!!!!!!!!\n\n");
            }

            if (!(VS is null) && VS.status != 2)
            {
                VS.Start(VCQueue, duration, UseBPAQueue, bpaqueuename, tags);

                return
                     "USER SET\r\n" +
                     "USER AUTHENTICATED\r\n" +
                     "WaitTime = " + SessionDuration + "\r\n" +
                     "STARTED\r\n";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                return
                     "USER SET\r\n" +
                     "USER AUTHENTICATED\r\n" +
                     "...\r\n" +
                     "UNAVALABLE";
            }
        }

        string DeleteSession(ConcurrentQueue<DBQueueObject> VCQueue, string sessionid)
        {

            VS = null;

            if (!(VS is null))
            {
                Console.WriteLine("\t\tDeleteSession::VS is not null\n");
            }

            //VCQueue.Enqueue(DBQueries.CreateSession(sessionid, processid, userid, port));

            DBQueueObject DBQueueObject = new DBQueueObject(this);
            DBQueueObject.Query = DBQueries.DeleteSession(sessionid);
           // DBQueueObject.needupdate = true;


            VCQueue.Enqueue(DBQueueObject);

            if (!(VS is null))
            {
                Console.WriteLine("\t\tDeleteSession::VS is null\n");
            }
            Console.ForegroundColor = ConsoleColor.Red;
            return
                   "USER SET\r\n" +
                   "USER AUTHENTICATED\r\n" +
                   "no\r\n" +
                   "SESSION DELETED : " + sessionid;
        }

    }


}


















