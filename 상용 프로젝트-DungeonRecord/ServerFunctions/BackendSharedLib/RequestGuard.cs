using BackEnd;
using System.IO;

namespace BackendSharedLib
{
    public static class RequestGuard
    {
        private const int KEEP_COUNT = 20;

        public static bool TryReplay(UserData userData, out Stream replay)
        {
            replay = null;
            var requestID = GetRequestID();
            if (requestID == null)
                return false;

            var log = userData.RequestLogs.Find(r => r.ID == requestID);
            if (log == null)
                return false;

            var param = new Param();
            param.Add("requestID", requestID);
            Backend.GameLog.InsertLogV2("Duplicate Request", param);
            replay = ReturnObject.Success(log.Result);
            return true;
        }

        public static string Commit(UserData userData, string resultJson)
        {
            var requestID = GetRequestID();
            if (requestID != null)
            {
                userData.RequestLogs.Add(new RequestRecord { ID = requestID, Result = resultJson });
                if (userData.RequestLogs.Count > KEEP_COUNT)
                    userData.RequestLogs.RemoveRange(0, userData.RequestLogs.Count - KEEP_COUNT);
            }

            var userParam = new Param();
            userParam.Add("userData", userData);
            WriteBatcher.ReplaceWrite("UserData", userParam);
            return WriteBatcher.Flush();
        }

        private static string GetRequestID()
        {
            return Backend.HasKey("requestID") ? Backend.Content["requestID"].ToString() : null;
        }
    }
}
