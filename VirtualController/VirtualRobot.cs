using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualController
{
    class VirtualRobot
    {
        int port;
        public int id;
        public string token;
        public VirtualSession VS;
        public VirtualRobot(int port, int id, string token)
        {
            this.port = port;
            this.id = id;
            this.token = token;
        }

        public String GetValues()
        {
            Console.WriteLine("Robot ID = " + id);
            Console.WriteLine("port:  " + port);
            Console.WriteLine("token = " + token);

            return "Robot ID = " + id + "\nport: " + port + "\ntoken = " + token + "";
        }

        public void CreateSession(int id, int time, int duration, string status)
        {
            this.VS = new VirtualSession(id, time, duration, status);
        }
    }
}
