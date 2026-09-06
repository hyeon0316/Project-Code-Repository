using System;
using System.Collections.Generic;
using BackEnd;
using Newtonsoft.Json;

namespace BackendFunction
{
    public static class AchievementParam
    {
        public static Param Get()
        {
            var param = new Param();
            var record = new HashSet<string>();
            param.Add("records", record);
            return param;
        }
    }
}