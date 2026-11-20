using Amazon.Lambda.Core;
using BackEnd;
using System;
using System.Collections.Generic;
using System.IO;
using BackendSharedLib;
using Newtonsoft.Json;
using System.Globalization;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BackendFunction
{
    public class BFunc
    {
        public Stream Function(Stream stream, ILambdaContext context)
        {
            try
            {
                Backend.Initialize(ref stream);
                if (WarmUp.IsRequest())
                    return ReturnObject.Success();
                ReadBatcher.Clear();
                WriteBatcher.Clear();

                string ADVENTUREGUIDE_KEY = "AdventureGuideData";
                string BATTLEPASS_KEY = "BattlePassData";
                string SHOP_KEY = "ShopRecordData";

                var isAdventureGuide = Backend.HasKey("adventureGuide");
                var seasonID = Backend.HasKey("seasonID") ? Backend.Content["seasonID"].ToString() : "";
                var shopTableID = Backend.HasKey("shopTableID") ? Backend.Content["shopTableID"].ToString() : "";

                var result = new UpdateResult();

                if (Backend.HasKey("inventoryConfigTableID"))
                {
                    try
                    {
                        var userBro = Backend.GameData.GetMyData("UserData", new Where());
                        if (!userBro.IsSuccess())
                            throw new Exception($"Get UserData failed: {userBro.ErrorCode}");
                        var userData = JsonConvert.DeserializeObject<UserData>(userBro.FlattenRows()[0]["userData"].ToJson());
                        if (userData.PendingGrants.Count > 0)
                        {
                            result.PendingReward = PendingGrantService.Grant(userData, new List<PendingGrant>(userData.PendingGrants),
                                Backend.Content["inventoryConfigTableID"].ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        ReadBatcher.Clear();
                        WriteBatcher.Clear();
                        var logParam = new Param();
                        logParam.Add("log", e.Message);
                        Backend.GameLog.InsertLogV2("Pending Grant Failed", logParam);
                    }
                }

                if (isAdventureGuide)
                    ReadBatcher.EnqueueRead(ADVENTUREGUIDE_KEY);
                if (!string.IsNullOrEmpty(seasonID))
                    ReadBatcher.EnqueueRead(BATTLEPASS_KEY);
                if (!string.IsNullOrEmpty(shopTableID))
                    ReadBatcher.EnqueueRead(SHOP_KEY);

                var readResult = ReadBatcher.Flush();
                if (readResult.ContainsKey("error"))
                    return ReturnObject.Error(readResult["error"].ToString());

                var now = DateTime.UtcNow;

                if (isAdventureGuide)
                {
                    var adventureGuideJson = readResult[ADVENTUREGUIDE_KEY]["record"].ToJson();
                    var adventureGuideRecord = JsonConvert.DeserializeObject<AdventureGuideRecord>(adventureGuideJson);

                    bool anchorPatched = false;

                    var curAnchor = ResetAchor.GetDaily(now);
                    if (string.IsNullOrEmpty(adventureGuideRecord.LastDailyAnchor))
                    {
                        adventureGuideRecord.LastDailyAnchor = curAnchor.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                        anchorPatched = true;
                    }
                    else
                    {
                        var lastAnchor = DateTime.Parse(adventureGuideRecord.LastDailyAnchor, null, DateTimeStyles.RoundtripKind);
                        if (curAnchor > lastAnchor)
                        {
                            adventureGuideRecord.LastDailyAnchor = curAnchor.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                            adventureGuideRecord.RewardClaimed.Clear();
                            result.IsDailyReset = true;
                        }
                    }

                    var curWeeklyAnchor = ResetAchor.GetWeekly(now);
                    if (string.IsNullOrEmpty(adventureGuideRecord.LastWeeklyAnchor))
                    {
                        adventureGuideRecord.LastWeeklyAnchor = curWeeklyAnchor.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                        anchorPatched = true;
                    }
                    else
                    {
                        var lastWeeklyAnchor = DateTime.Parse(adventureGuideRecord.LastWeeklyAnchor, null, DateTimeStyles.RoundtripKind);
                        if (curWeeklyAnchor > lastWeeklyAnchor)
                        {
                            adventureGuideRecord.LastWeeklyAnchor = curAnchor.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                            result.IsWeeklyReset = true;
                        }
                    }

                    if (result.IsDailyReset || result.IsWeeklyReset || anchorPatched)
                    {
                        var param = new Param();
                        param.Add("record", adventureGuideRecord);
                        WriteBatcher.EnqueueWrite(ADVENTUREGUIDE_KEY, param);
                    }
                }

                if (!string.IsNullOrEmpty(seasonID))
                {
                    var battlePassJson = readResult[BATTLEPASS_KEY]["record"].ToJson();
                    var battlePassRecord = JsonConvert.DeserializeObject<BattlePassRecord>(battlePassJson);
                    if (battlePassRecord.SeasonID != seasonID)
                    {
                        battlePassRecord.SeasonID = seasonID;
                        battlePassRecord.FreeClaimed.Clear();
                        battlePassRecord.PremiumClaimed.Clear();
                        battlePassRecord.IsPurchased = false;
                        var param = new Param();
                        param.Add("record", battlePassRecord);
                        WriteBatcher.EnqueueWrite(BATTLEPASS_KEY, param);
                        result.IsSeasonReset = true;
                    }
                }

                ShopEntry monthlyPassEntry = null;
                if (!string.IsNullOrEmpty(shopTableID))
                {
                    var productRecordJson = readResult[SHOP_KEY]["record"].ToJson();
                    var productRecords = JsonConvert.DeserializeObject<List<ProductRecord>>(productRecordJson);

                    var tableResult = ShopTable.Set(shopTableID);
                    if (!string.IsNullOrEmpty(tableResult))
                        return ReturnObject.Error(tableResult);

                    var entries = ShopTable.Entries;
                    var resetProducts = new List<ProductRecord>();
                    var expiredMonthlyPass = false;
                    productRecords.RemoveAll(p => !entries.ContainsKey(p.ProductID));

                    foreach (var p in productRecords)
                    {
                        if (string.IsNullOrEmpty(p.RefreshTimeUtc))
                            continue;

                        var refreshTime = DateTime.Parse(p.RefreshTimeUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        if (now < refreshTime)
                            continue;

                        var entry = entries[p.ProductID];
                        if (p.ProductID == "monthly_pass")
                        {
                            p.RemainLimitCount -= (int)(now - refreshTime).TotalDays;
                            if (p.RemainLimitCount <= 0)
                            {
                                p.RemainLimitCount = 0;
                                expiredMonthlyPass = true;
                                resetProducts.Add(p);
                                continue;
                            }
                            p.RemainLimitCount--;
                            monthlyPassEntry = entry;
                            result.MonthlyPassRemainCount = p.RemainLimitCount;
                        }
                        else
                        {
                            p.RemainLimitCount = entry.LimitCount;
                        }
                        p.RefreshTimeUtc = RefreshAnchor.ComputeNextRefreshUtc(entry.RefreshType, now);
                        resetProducts.Add(p);
                    }

                    if (expiredMonthlyPass)
                        productRecords.RemoveAll(p => p.ProductID == "monthly_pass" && p.RemainLimitCount == 0);

                    if (resetProducts.Count > 0 || expiredMonthlyPass)
                    {
                        result.ResetProducts = resetProducts.ConvertAll(p => p.ProductID);
                        var param = new Param();
                        param.Add("record", productRecords);
                        WriteBatcher.EnqueueWrite(SHOP_KEY, param);
                    }
                }

                if (monthlyPassEntry != null)
                {
                    var inventoryConfigTableID = Backend.Content["inventoryConfigTableID"].ToString();
                    result.MonthlyPassReward = RewardService.GiveReward(
                        new List<TransactionItemSpec> { monthlyPassEntry.RewardSpecs[1] },
                        RewardFullPolicy.Overflow,
                        inventoryConfigTableID);
                }
                else
                {
                    var flush = WriteBatcher.Flush();
                    if (!string.IsNullOrEmpty(flush))
                        return ReturnObject.Error(flush);
                }

                return ReturnObject.Success(JsonConvert.SerializeObject(result));
            }
            catch (RewardException re)
            {
                return ReturnObject.Error(re.Code, re.Detail);
            }
            catch (Exception e)
            {
                return ReturnObject.Error(8001, e.Message);
            }
        }
    }
}
