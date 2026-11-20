using System;
using System.Collections.Generic;
using System.IO;
using BackEnd;
using BackendSharedLib;
using Newtonsoft.Json;

namespace BackendFunction
{
    public static class RealPurchase
    {
        public static Stream Run(string inventoryConfigTableID)
        {
            var productID = Backend.Content["productID"].ToString();
            var token = Backend.Content["token"].ToString();

            if (!ShopTable.Entries.TryGetValue(productID, out var target))
                return ReturnObject.Error(5000, $"Unknown productID : {productID}");

            if (target.CurrencyID != 0)
            {
                var suspectParam = new Param();
                suspectParam.Add("msg", "실결제 요청인데 Virtual 전용 상품(CurrencyID 설정됨)");
                suspectParam.Add("productID", productID);
                suspectParam.Add("currencyID", target.CurrencyID);
                return ReturnObject.Suspect(suspectParam);
            }

            var bro = Backend.Receipt.IsValidateGooglePurchase(productID, token, "뒤끝펑션 구글 영수증 검증", false);
            if (!bro.IsSuccess())
            {
                var logParam = new Param();
                logParam.Add("log", bro.ToString());
                Backend.GameLog.InsertLogV2("Potential Cheater", logParam);
                return ReturnObject.Error($"{bro.ErrorCode} : {bro.Message}");
            }

            if (target.ProductID == "battle_pass")
            {
                var bpBro = Backend.GameData.GetMyData("BattlePassData", new Where());
                if (!bpBro.IsSuccess())
                    return ReturnObject.Error($"{bpBro.ErrorCode} : {bpBro.Message}");
                var bpRecord = JsonConvert.DeserializeObject<BattlePassRecord>(
                    bpBro.FlattenRows()[0]["record"].ToJson());
                bpRecord.IsPurchased = true;

                var bpParam = new Param();
                bpParam.Add("record", JsonConvert.SerializeObject(bpRecord));
                WriteBatcher.EnqueueWrite("BattlePassData", bpParam);

                var writeResult = WriteBatcher.Flush();
                if (!string.IsNullOrEmpty(writeResult))
                    return ReturnObject.Error(writeResult);

                return ReturnObject.Success("{}");
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
                    records.Add(new ProductRecord
                    {
                        ProductID = target.ProductID,
                        RefreshTimeUtc = RefreshAnchor.ComputeNextRefreshUtc(target.RefreshType, DateTime.UtcNow),
                        RemainLimitCount = target.ProductID == "monthly_pass" ?
                            target.LimitCount : target.LimitCount - 1,
                    });
                }
                else
                {
                    if (target.ProductID == "monthly_pass")
                    {
                        record.RemainLimitCount += target.LimitCount;
                    }
                    else
                    {
                        if (record.RemainLimitCount < 1)
                        {
                            var param = new Param();
                            param.Add("productID", productID);
                            param.Add("msg", "구매 횟수 제한");
                            return ReturnObject.Suspect(param);
                        }
                        record.RemainLimitCount -= 1;
                    }
                }

                var recordParam = new Param();
                recordParam.Add("shopRecords", records);
                WriteBatcher.EnqueueWrite("ShopRecordData", recordParam);
            }

            List<TransactionItemSpec> rewardsToGive = target.ProductID == "monthly_pass" ?
                new List<TransactionItemSpec> { target.RewardSpecs[0] } : target.RewardSpecs;
            var change = RewardService.GiveReward(rewardsToGive, RewardFullPolicy.Overflow, inventoryConfigTableID);
            return ReturnObject.Success(JsonConvert.SerializeObject(change));
        }
    }
}
