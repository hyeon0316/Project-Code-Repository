using System;
using BackEnd;
using BackendSharedLib;

namespace BackendFunction
{
    public static class AdventureGuideParam
    {
        public static Param Get()
        {
            var serverTime = DateTime.UtcNow;
            var record = new AdventureGuideRecord();
            record.LastDailyAnchor = RefreshAnchor.ComputePreviousResetUtc("Daily", serverTime);
            record.LastWeeklyAnchor = RefreshAnchor.ComputePreviousResetUtc("Weekly", serverTime);
            var param = new Param();
            param.Add("record", record);
            return param;
        }
    }
}