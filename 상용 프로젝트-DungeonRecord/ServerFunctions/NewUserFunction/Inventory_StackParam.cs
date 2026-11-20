using System;
using System.Collections.Generic;
using BackEnd;
using Newtonsoft.Json;

namespace BackendFunction
{
    public static class Inventory_StackParam
    {
        public static Param Get()
        {
            var param = new Param();
            var dic = new Dictionary<int, int>();
            param.Add("items", dic);
            return param;
        }
    }
}