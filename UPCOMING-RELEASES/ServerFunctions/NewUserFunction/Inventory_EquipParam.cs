using System;
using System.Collections.Generic;
using BackEnd;
using Newtonsoft.Json;

namespace BackendFunction
{
    public static class Inventory_EquipParam
    {
        public static Param Get()
        {
            var param = new Param();
            var dic = new Dictionary<long, string>();
            param.Add("weapons", dic);
            param.Add("wearables", dic);
            return param;
        }
    }
}