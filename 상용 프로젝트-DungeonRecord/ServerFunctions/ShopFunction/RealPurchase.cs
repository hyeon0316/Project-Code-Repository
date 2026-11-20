using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackEnd;
using BackendSharedLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendFunction
{
    public class ReceiptGrant
    {
        public string Token;

        /// <summary>
        /// 검증 성공 후 지급 쓰기 전. 기본값 false(=지급 완료)로 두어 
        /// 필드가 없는 기록이 재지급되지 않게 한다.
        /// </summary>
        public bool IsPending;
        public List<string> Stackables;
    }

    public static class RealPurchase
    {
        private const int RECEIPT_TOKEN_KEEP_COUNT = 50;

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

            ReadBatcher.EnqueueRead("ShopRecordData");
            var readResult = ReadBatcher.Flush();
            if (readResult.ContainsKey("error"))
                return ReturnObject.Error(readResult["error"].ToString());

            var shopRecordRow = readResult["ShopRecordData"];
            var receiptGrants = JsonConvert.DeserializeObject<List<ReceiptGrant>>(shopRecordRow["receiptGrants"].ToJson());

            var grant = receiptGrants.Find(g => g.Token == token);

            // 지급까지 끝난 구매. 그때의 지급 결과를 함께 돌려준다.
            // 클라는 이번 세션에 응답이 유실된 주문이면 이 결과로 로컬을 반영하고, 아니면 Confirm만 한다.
            if (grant != null && !grant.IsPending)
            {
                var grantedResult = new TransactionResult();
                foreach (var entry in grant.Stackables)
                {
                    var parts = entry.Split(':');
                    grantedResult.StackableDic[int.Parse(parts[0])] = int.Parse(parts[1]);
                }
                var grantedJson = JObject.FromObject(grantedResult);
                grantedJson["alreadyGranted"] = true;
                return ReturnObject.Success(grantedJson.ToString(Formatting.None));
            }

            // pending 기록이 있으면 이전 호출에서 검증은 성공했고 지급 쓰기만 실패한 것이다.
            // 검증은 token을 사용 처리해 다시 하면 UsedReceipt가 되므로 건너뛰고 지급만 다시 한다.
            if (grant == null)
            {
                var bro = Backend.Receipt.IsValidateGooglePurchase(productID, token, "뒤끝펑션 구글 영수증 검증", false);
                if (!bro.IsSuccess())
                {
                    var logParam = new Param();
                    logParam.Add("log", bro.ToString());

                    // 검증으로 사용 처리됐지만 기록이 없는 token. pending 기록 쓰기 실패 또는 타인 token 재사용이다.
                    // 클라가 Confirm하지 않도록 에러로 응답해 Google 자동 환불에 맡긴다.
                    if (bro.ErrorCode == "UsedReceipt")
                    {
                        logParam.Add("productID", productID);
                        Backend.GameLog.InsertLogV2("UsedReceipt Not Granted", logParam);
                        return ReturnObject.Error(7002, $"{bro.ErrorCode} : {bro.Message}");
                    }

                    Backend.GameLog.InsertLogV2("Potential Cheater", logParam);
                    return ReturnObject.Error(5000, $"{bro.ErrorCode} : {bro.Message}");
                }

                grant = new ReceiptGrant { Token = token, IsPending = true, Stackables = new List<string>() };
                AddReceiptGrant(receiptGrants, grant);

                var pendingParam = new Param();
                pendingParam.Add("receiptGrants", receiptGrants);
                WriteBatcher.EnqueueWrite("ShopRecordData", pendingParam);
                var pendingResult = WriteBatcher.Flush();
                if (!string.IsNullOrEmpty(pendingResult))
                {
                    var failParam = new Param();
                    failParam.Add("productID", productID);
                    failParam.Add("log", pendingResult);
                    Backend.GameLog.InsertLogV2("Validated Not Recorded", failParam);
                    return ReturnObject.Error(pendingResult);
                }
            }

            var records = JsonConvert.DeserializeObject<List<ProductRecord>>(shopRecordRow["record"].ToJson());

            if (target.ProductID == "battle_pass")
            {
                var bpBro = Backend.GameData.GetMyData("BattlePassData", new Where());
                if (!bpBro.IsSuccess())
                    return ReturnObject.Error($"{bpBro.ErrorCode} : {bpBro.Message}");
                var bpRecord = JsonConvert.DeserializeObject<BattlePassRecord>(
                    bpBro.FlattenRows()[0]["record"].ToJson());
                bpRecord.IsPurchased = true;

                var bpParam = new Param();
                bpParam.Add("record", bpRecord);
                WriteBatcher.EnqueueWrite("BattlePassData", bpParam);
                EnqueueShopRecordWrite(records, receiptGrants, grant, new TransactionResult());

                var bWriteResult = WriteBatcher.Flush();
                if (!string.IsNullOrEmpty(bWriteResult))
                    return ReturnObject.Error(bWriteResult);

                return ReturnObject.Success("{}");
            }

            var isNewMonthlyPass = false;
            if (target.LimitCount != 0)
            {
                var record = records.Find(x => x.ProductID == target.ProductID);
                if (record == null)
                {
                    records.Add(new ProductRecord
                    {
                        ProductID = target.ProductID,
                        RefreshTimeUtc = RefreshAnchor.ComputeNextRefreshUtc(target.RefreshType, DateTime.UtcNow),
                        RemainLimitCount = target.LimitCount - 1,
                    });
                    isNewMonthlyPass = target.ProductID == "monthly_pass";
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
            }

            List<TransactionItemSpec> rewardsToGive = target.RewardSpecs;
            if (target.ProductID == "monthly_pass")
            {
                rewardsToGive = isNewMonthlyPass ?
                    new List<TransactionItemSpec> { target.RewardSpecs[0], target.RewardSpecs[1] } :
                    new List<TransactionItemSpec> { target.RewardSpecs[0] };
            }

            var change = RewardService.GiveReward(rewardsToGive, RewardFullPolicy.Overflow, inventoryConfigTableID, flush: false);
            EnqueueShopRecordWrite(records, receiptGrants, grant, change);

            var writeResult = WriteBatcher.Flush();
            if (!string.IsNullOrEmpty(writeResult))
                return ReturnObject.Error(writeResult);

            return ReturnObject.Success(JsonConvert.SerializeObject(change));
        }

        private static void AddReceiptGrant(List<ReceiptGrant> receiptGrants, ReceiptGrant grant)
        {
            receiptGrants.Add(grant);
            if (receiptGrants.Count > RECEIPT_TOKEN_KEEP_COUNT)
                receiptGrants.RemoveRange(0, receiptGrants.Count - RECEIPT_TOKEN_KEEP_COUNT);
        }

        private static void EnqueueShopRecordWrite(List<ProductRecord> records, List<ReceiptGrant> receiptGrants,
            ReceiptGrant grant, TransactionResult result)
        {
            grant.IsPending = false;
            grant.Stackables = result.StackableDic.Select(p => $"{p.Key}:{p.Value}").ToList();

            var recordParam = new Param();
            recordParam.Add("record", records);
            recordParam.Add("receiptGrants", receiptGrants);
            WriteBatcher.EnqueueWrite("ShopRecordData", recordParam);
        }
    }
}
