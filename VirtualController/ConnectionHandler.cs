using CSharpTest.Net.Http;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace ru.pflb.VirtualController
{
    public static class ConnectionHandler
    {
        public static void PrintKeysAndValues(NameValueCollection myCol)
        {
            Console.WriteLine("   KEY        VALUE");
            foreach (String s in myCol.AllKeys)
                Console.WriteLine("   {0,-10} {1}", s, myCol[s]);
            Console.WriteLine();
        }
        public static NameValueCollection KeysAndValuesFromBody(Stream stream)
        {

            NameValueCollection BodyCol = new NameValueCollection();
            string BodyString = ToString(stream);
            BodyCol = HttpUtility.ParseQueryString(BodyString);
            return BodyCol;
        }
        public static void SimpleTextResponse(HttpListenerContext context, string Answer)
        {            
            HttpListenerResponse response = context.Response;
            string ResponseString = Answer;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(ResponseString);
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public static Stream ToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string ToString(Stream stream)
        {

            StreamReader reader = new StreamReader(stream);
            string String = reader.ReadToEnd();
            return String;
        }

        public static string Path(string url)
        {
            if (url.IndexOf('?') > 0)
                url = url.Substring(0, url.IndexOf('?'));
            return url;
        }
        public static void ConnectionInfo(HttpListenerRequest request)
        {
              Console.WriteLine("URL: {0}", request.Url.OriginalString);
              Console.WriteLine("Raw URL: {0}", request.RawUrl);
              Console.WriteLine("Path: {0}", Path(request.RawUrl));
              Console.WriteLine("method: {0}", request.HttpMethod);
              Console.WriteLine("{0} request was caught: {1}",
              request.HttpMethod, request.Url);
              Console.WriteLine("Query: {0}", request.QueryString);
        }
               
    }
}
