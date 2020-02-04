using System;
using System.Collections.Generic;
using System.Text;

namespace ru.pflb.VirtualController
{

    static class DBQueries
    {
        static string tagString(string tags)
        {
            string[] tagArray = tags.Split(";");

            string tagStringInclude = "tag in (";
            string tagStringNotInclude = "tag not in (";
            //string[] animals = new string[3];

            bool tagStringIncludeExists = false;
            bool tagStringNotIncludeExists = false;

            for (int i = 0; i < tagArray.Length; i++)
            {
                if (tagArray[i].Contains('-'))
                {
                    tagStringNotIncludeExists = true;

                    tagStringNotInclude = tagStringNotInclude + "'" + tagArray[i] + "'";
                }
                if (!(tagArray[i].Contains('-')))

                {

                    tagStringIncludeExists = true;
                    tagStringInclude = tagStringInclude + "'" + tagArray[i] + "'";
                }

            }

            string tagString = "";

            if ((tagStringIncludeExists) & (tagStringNotIncludeExists))
            {
                tagStringInclude = tagStringInclude + ") and ";
            }
            else
            {
                tagStringInclude = tagStringInclude + ")";
            }

            tagStringNotInclude = tagStringNotInclude + ")";

            if (!tagStringIncludeExists) { tagStringInclude = ""; };
            if (!tagStringNotIncludeExists) { tagStringNotInclude = ""; };

            tagString = tagStringInclude + tagStringNotInclude;

            tagString = tagString.Replace("''", "','");
            tagString = tagString.Replace("-", "");

            return tagString;

        }
        static string BPAResourceStatus(int status_id)
        {
            switch (status_id)
            {
                case 0: return "Offline";
                case 1: return "Idle";
                case 2: return "Ready";
                default: return "(not found)";
            }
        }

        static string BPADisplayStatus(int status_id)
        {
            switch (status_id)
            {
                case 0: return "Private";
                case 1: return "Private";
                case 2: return "Offline";
                default: return "(not found)";
            }
        }


        static string SessionStatus(int status_id)
        {
            /* switch (status_id)
             {
                 case 0: return "0 (Pending)";
                 case 1: return "1 (Running)";
                 case 2: return "2 (Finished)";
                 default: return "(not found)";
             } */
            return status_id.ToString();
        }

        public static string CreateToken(string userid, string VR_token)
        {

            String RequestString = "insert into BPAInternalAuth(UserID, Token, Expiry, Roles, LoggedInMode, isWebService)" +
                       "values('" + userid + "', '" + VR_token + "', CURRENT_TIMESTAMP, 10, 2, 0); ";
            return RequestString;
        }

        public static string CreateSession(string sessionid, string processid, string userid, int VR_port, string BPAPrefix)
        {

 //           String resourceid = "(select resourceid from BPAResource where name = 'tvsi-rpa0012:" + VR_port.ToString() + "')";

            String resourceid = "(select resourceid from BPAResource where name = '" + BPAPrefix + ":" + VR_port.ToString() + "')";
            String RequestString = "insert into BPASession(sessionid, startdatetime, processid, starteruserid," +
                                   "runningresourceid, starterresourceid, statusid, starttimezoneoffset, startparamsxml)" +
                                   "values('" + sessionid + "', CURRENT_TIMESTAMP, '" + processid + "','" + userid + "',"
                                   + resourceid + "," + resourceid + ",'" + SessionStatus(0) + "', '10800', '<inputs />' )";
            return RequestString;
        }


        public static string RebuildSessions()
        {
            String RequestString = "update BPASession " +
               "set statusid = 2";

            return RequestString;

        }


        public static string UpdateSession(string sessionid, int statusid)
        {
            String RequestString = "update BPASession " +
               "set statusid = '" + SessionStatus(statusid) + "', startparamsxml = '<inputs />' " +
               "where sessionid = '" + sessionid + "';";
            return RequestString;

        }

        public static string UpdateSessionEndTime(string sessionid, int statusid)
        {
            String RequestString = "update BPASession " +
               "set statusid = '" + SessionStatus(statusid) + "', enddatetime = CURRENT_TIMESTAMP, startparamsxml = '<inputs />' " +
               "where sessionid = '" + sessionid + "';";
            return RequestString;

        }


        public static string DeleteSession(string sessionid)
        {
            String RequestString = "delete from BPASession " +
               "where sessionid = '" + sessionid + "';";
            return RequestString;

        }

        public static string CreateBPAResource(string resourceid, int port)
        {

                  string FQDN = "tvsi-rpa0013";
            //string FQDN = "tvsi-rpa0012";
            string name = FQDN + ":" + port.ToString();
            FQDN = FQDN + ".delta.sbrf.ru"; // по умолчанию 8181
            String RequestString =
                     "       insert into BPAResource ( " +
                     "  resourceid " +
                     " ,name " +
                     " ,status " +
                     " ,processesrunning " +
                     " ,actionsrunning " +
                     " ,unitsallocated " +
                     " ,lastupdated " +
                     " ,AttributeID " +
                     " ,diagnostics " +
                     " ,logtoeventlog " +
                     " ,FQDN " +
                     " ,ssl " +
                     " ,userID " +
                     ") values ( '" +
                     resourceid + "' , '" +
                     name + "' , '" +
                     BPAResourceStatus(0) +
                     "' , 0, 0, 0, CURRENT_TIMESTAMP, 32,  0, 1 , '" +
                     FQDN + "', 0, '5AE9EC53-6B7B-45B5-89DA-6C29F2345196')";

            return RequestString;

        }

        public static string UpdateBPAResource(int port, int BPAStatus, int processesrunning, string BPAPrefix)
        {

            //string FQDN = "tvsi-rpa0012";
            string FQDN = BPAPrefix;
            string name = FQDN + ":" + port.ToString();


            String RequestString = "update BPAResource " +
               "set status = '" + BPAResourceStatus(BPAStatus) + "' , lastupdated = CURRENT_TIMESTAMP, " +
               "processesrunning = " + processesrunning + " , actionsrunning = " + processesrunning +
               " where name = '" + name + "';";

            return RequestString;

        }


        public static string UpdateBPAResourcesLastUpdated()
        {
            String RequestString = "update BPAResource " +
               "set lastupdated = GETUTCDATE() " +
               "where name like '%:%'";
            return RequestString;

        }

        public static string BPAWorkQueueItem(string sessionid, string queuename, string tag)
        {

            /*String RequestString = "update BPAWorkQueueItem " +
               "set completed = CURRENT_TIMESTAMP" +
               " where sessionid = '" + sessionid + "';";

*/          String RequestString = "";
            if (tag == "")
            {
                RequestString = "update BPAWorkQueueItem " +
                            "set completed = GETUTCDATE(), sessionid = '" + sessionid +
                            "' where id in (select top (1) id from BPAWorkQueueItem with (updlock)" +
                            " where completed is null" +
                            " and queueid in (select id from bpaworkqueue where name = '" + queuename + "') " +
                            " and ident not in (select queueitemident from bpaworkqueueitemtag) " +
                            " and ident not in (select id from bpacaselock where sessionid <> '" + sessionid + "')" +
                            " order by loaded asc)";
            }
            else
            {
                /*
                RequestString = "update BPAWorkQueueItem " +
                             "set completed = GETUTCDATE(), sessionid = '" + sessionid +
                             "' where id in (select top (1) id from BPAWorkQueueItem" +
                             "left join bpaworkqueueitemtag on bpaworkqueueitem.ident = bpaworkqueueitemtag.queueitemident " +
                             " where completed is null" +
                             " and queueid in (select id from bpaworkqueue where name = '" + queuename + "') " +
                 //            " and ident in (select queueitemident from bpaworkqueueitemtag " +
                             " and ident not in (select id from bpacaselock where sessionid <> '" + sessionid + "')" +
                             " where (tagid in (select id from bpatag where " + tagString(tag) + ") or tagid is null)" +
                             " order by loaded asc)";
                             */

                RequestString = "update BPAWorkQueueItem " +
                                 "set completed = GETUTCDATE(), sessionid = '" + sessionid +
                                 "' where id in (select top (1) id from BPAWorkQueueItem with (updlock)" +
                                 " left join bpaworkqueueitemtag on bpaworkqueueitem.ident = bpaworkqueueitemtag.queueitemident " +
                                 " where completed is null" +
                                 " and queueid in (select id from bpaworkqueue where name = '" + queuename + "') " +
                                 //            " and ident in (select queueitemident from bpaworkqueueitemtag " +
                                 " and ident not in (select id from bpacaselock where sessionid <> '" + sessionid + "')" +
                                 " and (tagid in (select id from bpatag where " + tagString(tag) + ") or tagid is null)" +
                                 " order by loaded asc)";

            }

            return RequestString;
        }

        public static string InsertBPACaseLock(string sessionid, string queuename, string tag)
        {

            string queueident = "";

            if (tag != "")
            {
                /*
                queueident = "(select top (1) ident from BPAWorkQueueItem" +
                                 " left join bpaworkqueueitemtag on bpaworkqueueitem.ident = bpaworkqueueitemtag.queueitemident" +
                                 " where completed is null" +
                                 " and queueid in (select id from bpaworkqueue where name = '" + queuename + "') " +
                                 " and ident in (select queueitemident from bpaworkqueueitemtag " +
                                 " where tagid in (select id from bpatag where " + tagString(tag) + "))" +
                                 " and ident not in (select id from bpacaselock)" +
                                 " order by loaded asc)";
                                 */
                queueident = "(select top (1) ident from BPAWorkQueueItem with (updlock)" +
                " left join bpaworkqueueitemtag on bpaworkqueueitem.ident = bpaworkqueueitemtag.queueitemident " +
                                     " where completed is null" +
                                     " and queueid in (select id from bpaworkqueue where name = '" + queuename + "') " +
                                     //            " and ident in (select queueitemident from bpaworkqueueitemtag " +
                                     " and ident not in (select id from bpacaselock)" +
                                    // " where sessionid <> '" + sessionid + "')" +
                                     " and (tagid in (select id from bpatag where " + tagString(tag) + ") or tagid is null)" +
                                     " order by loaded asc)";

            }
            else
            {
                queueident = "(select top (1) ident from BPAWorkQueueItem with (updlock)" +
                                " where completed is null" +
                                " and queueid in (select id from bpaworkqueue where name = '" + queuename + "') " +
                                " and ident not in (select id from bpacaselock)" +
                                " order by loaded asc)";
            }

            String RequestString = "if (" + queueident + "is not null) " +
                "Insert into BPACaselock (id, locktime, sessionid, lockid) " +
               "values (" + queueident + ", GETUTCDATE(), '" + sessionid + "', newid())";

            return RequestString;

        }


        public static string DeleteFromBPACaseLock(string sessionid)
        {

            String RequestString = " delete from BPACaselock " +
               "where sessionid = '" + sessionid + "'";

            return RequestString;

        }

        public static string InsertIntoInflux(string responsetime, string InstanceName)
        {

            String RequestString = "insert rr, robot_port = " + InstanceName +
               " response_time = " + responsetime;

            return RequestString;

        }


    }
}
