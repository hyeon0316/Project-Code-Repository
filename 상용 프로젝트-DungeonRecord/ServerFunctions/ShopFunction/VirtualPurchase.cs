using System;
using System.Collections.Generic;
using System.IO;
using BackEnd;
using BackendSharedLib;
using Newtonsoft.Json;

namespace BackendFunction
{
    public static class VirtualPurchase
    {
        public static Stream Run(string inventoryConfigTableID)
        {
            var productID = Backend.Content["productID"].ToString();
            int purchaseCount = int.Parse(Backend.Content["purchaseCount"].ToString());

            if (!ShopTable.Entries.TryGetValue(productID, out var target))
                return ReturnObject.Error(5000, $"Invalid product id : {productID}");

            if (target.CurrencyID == 0)
            {
                var suspectParam = new Param();
                suspectParam.Add("msg", "재화결제 요청인데 CurrencyID 미설정(실결제 전용 상품)");
                suspectParam.Add("productID", productID);
                return ReturnObject.Suspect(suspectParam);
            }

            if (target.LimitCount != 0)
            {
                ReadBatcher.EnqueueRead("ShopRecordData");
                var readResult = ReadBatcher.Flush();
                if (readResult.ContainsKey("error"))
                    return ReturnObject.Error(readResult["error"].ToString());

                var records = JsonConvert.DeserializeObject<List<ProductRecord>>
                    (readResult["ShopRecordData"]["shopRecords"].ToJson());
                var record = records.Find(x => x.ProductID == target.ProductID);
                if (record == null)
                {
                    record = new ProductRecord
                    {
                        ProductID = target.ProductID,
                        RefreshTimeUtc = RefreshAnchor.ComputeNextRefreshUtc(target.RefreshType, DateTime.UtcNow),
                        RemainLimitCount = target.LimitCount,
                    };
                    records.Add(record);
                }
                if (record.RemainLimitCount < purchaseCount)
                {
                    var param = new Param();
                    param.Add("productID", productID);
                    param.Add("purchaseCount", purchaseCount);
                    param.Add("msg", "구매 횟수 제한");
                    return ReturnObject.Suspect(param);
                }
                record.RemainLimitCount -= purchaseCount;
                var recordParam = new Param();
                recordParam.Add("shopRecords", records);
                WriteBatcher.EnqueueWrite("ShopRecordData", recordParam);
            }

            var costs = new List<TransactionItemSpec>
            {
                new() { ID = target.CurrencyID, Amount = target.Price * purchaseCount }
            };

            var change = RewardService.GiveReward(
                target.RewardSpecs,
                RewardFullPolicy.Overflow,
                inventoryConfigTableID,
                userData: null,
                costs: costs);

            return ReturnObject.Success(JsonConvert.SerializeObject(change));
        }
    }
}
