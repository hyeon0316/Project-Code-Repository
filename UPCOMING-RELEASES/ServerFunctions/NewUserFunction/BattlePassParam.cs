using System;
using System.Collections.Generic;
using BackEnd;
using BackendSharedLib;

namespace BackendFunction
{
    public static class BattlePassParam
    {
        public static Param Get(string seasonID)
        {
            var param = new Param();
            var record = new BattlePassRecord();
            record.SeasonID = seasonID;
            param.Add("record", record);
            return param;
        }
    }
}