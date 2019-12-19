using System;
using System.Collections.Generic;
using System.Text;

namespace ru.pflb.VirtualController
{

    static class DBQueries
    {
        public static string Status(int status_id)
        {
            switch (status_id)
            {
                case 0: return "0 (Pending)";
                case 1: return "1 (Running)";
                case 2: return "2 (Finished)";
                default: return "(not found)";
            }         
         }

        public static string CreateToken(string userid, string VR_token)
        {

            String RequestString = "insert into BPAInternalAuth(UserID, Token, Expiry, Roles, LoggedInMode, isWebService)" +
                       "values('" + userid + "', '" + VR_token + "', CURRENT_TIMESTAMP, 10, 2, 0); ";
            return RequestString;
        }

        public static string CreateSession(string sessionid, string processid, string userid, string VR_id)
        {

            String RequestString = "insert into BPASession(sessionid, startdatetime, processid, starteruserid," +
                                   "runningresourceid, starterresourceid, statusid, starttimeoffsetzone)" +
                                   "values('" + sessionid + "', CURRENT_TIMESTAMP, '" + processid + "','" + userid + "','"
                                   + VR_id + "','" + VR_id + "','" + Status(0) + "', '10800' )";
            return RequestString;
        }

        public static string UpdateSession(string sessionid, int statusid)
        {
            String RequestString = "update BPASession " +
               "set statusid = '" + Status(statusid) + "' " +
               "where sessionid = '" + sessionid + "';";


            return RequestString;

        }

        /*
        public static string DBAResourceCreate()
        {
            string RequestString;
            return RequestString;


        }*/

    }
}
