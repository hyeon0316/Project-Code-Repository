using System;
using System.Collections.Generic;
using BackEnd;
using BackendSharedLib;

namespace BackendFunction
{
    public static class ShopParam
    {
        public static Param Get(string tableID)
        {
            var bro = Backend.Chart.GetChartContents(tableID);
            if (!bro.IsSuccess())
                throw new Exception($"{bro.ErrorCode} : {bro.Message}");

            var serverTime = DateTime.UtcNow;

            var records = new List<ProductRecord>();
            var jsonData = bro.FlattenRows();
            for (int i = 0; i < jsonData.Count; i++)
            {
                var refreshType = jsonData[i]["RefreshType"].ToString();
                int limitCount = int.Parse(jsonData[i]["LimitCount"].ToString());
                string productID = jsonData[i]["ProductID"].ToString();
                if (limitCount == 0 || productID == "monthly_pass") //monthly만 limitCount를 다른 용도로 쓰임
                    continue;

                var record = new ProductRecord();
                record.ProductID = productID;
                record.RefreshTimeUtc = RefreshAnchor.ComputeNextRefreshUtc(refreshType, serverTime);
                record.RemainLimitCount = limitCount;
                records.Add(record);
            }
            var param = new Param();
            param.Add("record", records);
            return param;
        }
    }
}