using System;
using System.Collections.Generic;
using System.Text;

namespace ru.pflb.VirtualController
{
    static class DBQueries
    {

        public static string CreateToken(string userid, string VR_token)
        {

            String RequestString = "insert into BPAInternalAuth(UserID, Token, Expiry, Roles, LoggedInMode, isWebService)" +
                       "values('" + userid + "', '" + VR_token + "', CURRENT_TIMESTAMP, 10, 2, 0); ";
            return RequestString;
        }

        public static string CreateSession(string sessionid, string processid, string userid, string VR_id)
        {

            String status = "(Pending)";
            String RequestString = "insert into BPASession(sessionid, startdatetime, processid, starteruserid," +
                                   "runningresourceid, starterresourceid, statusid, starttimeoffsetzone)" +
                                   "values('" + sessionid + "', CURRENT_TIMESTAMP, '" + processid + "','" + userid + "','"
                                   + VR_id + "','" + VR_id + "','" + status + "', '10800' )";
            return RequestString;
        }

        public static string UpdateSession(string sessionid, string status)
        {
                        String RequestString = "update BPASession " +
                           "set statusid = '" + status + "' " +
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
