using System;
using System.Collections.Generic;
using BackEnd;
using Newtonsoft.Json;

namespace BackendFunction
{
    public static class QuestParam
    {
        public static Param Get()
        {
            var param = new Param();
            param.Add("record", new List<string>());
            return param;
        }

    }
}