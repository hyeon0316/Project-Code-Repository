using Amazon.Lambda.Core;
using BackEnd;
using System;
using System.Collections.Generic;
using System.IO;
using BackendSharedLib;
using Newtonsoft.Json;

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

                var ruleTableID = Backend.Content["ruleTableID"].ToString();
                var bannerTableID = Backend.Content["bannerTableID"].ToString();
                var characterPoolTableID = Backend.Content["characterPoolTableID"].ToString();
                var aWeaponPoolTableID = Backend.Content["aWeaponPoolTableID"].ToString();
                var bWeaponPoolTableID = Backend.Content["bWeaponPoolTableID"].ToString();
                var inventoryConfigTableID = Backend.Content["inventoryConfigTableID"].ToString();
                var isNormalBanner = Backend.HasKey("isNormal");
                int drawCount = Backend.HasKey("isTenPull") ? 10 : 1;

                var tableResult = GachaRuleTable.Set(ruleTableID);
                if (!string.IsNullOrEmpty(tableResult))
                    return ReturnObject.Error(tableResult);

                ReadBatcher.EnqueueRead("UserData");
                ReadBatcher.EnqueueRead("OwnedCharacterData");
                var readResult = ReadBatcher.Flush();
                if (readResult.ContainsKey("error"))
                    return ReturnObject.Error(readResult["error"].ToString());

                var userData = JsonConvert.DeserializeObject<UserData>(readResult["UserData"]["userData"].ToJson());
                var characterRecords = JsonConvert.DeserializeObject<List<CharacterRecord>>(readResult["OwnedCharacterData"]["characters"].ToJson());

                int ownedTicket = userData.Balances.GetValueOrDefault((int)CurrencyKey.GachaTicket);
                if (ownedTicket < drawCount)
                {
                    var sp = new Param();
                    sp.Add("msg", "가챠 재화 부족");
                    sp.Add("itemID", CurrencyKey.GachaTicket);
                    sp.Add("owned", ownedTicket);
                    sp.Add("required", drawCount);
                    return ReturnObject.Suspect(sp);
                }

                if (!isNormalBanner)
                {
                    var bannerResult = GachaBannerTable.Set(bannerTableID);
                    if (!string.IsNullOrEmpty(bannerResult))
                        return ReturnObject.Error(bannerResult);

                    var now = DateTime.UtcNow;
                    if (now < GachaBannerTable.Banner.StartDate || now > GachaBannerTable.Banner.EndDate)
                    {
                        var sp = new Param();
                        sp.Add("msg", "한정 가챠 시즌 외 접근");
                        sp.Add("bannerID", GachaBannerTable.Banner.BannerID);
                        sp.Add("serverTime", now.ToString("o"));
                        sp.Add("startDate", GachaBannerTable.Banner.StartDate.ToString("o"));
                        sp.Add("endDate", GachaBannerTable.Banner.EndDate.ToString("o"));
                        return ReturnObject.Suspect(sp);
                    }
                }

                userData.Balances[(int)CurrencyKey.GachaTicket] = ownedTicket - drawCount;

                var pullResults = new List<GachaDraw.Result>();
                var weaponRewards = new List<TransactionItemSpec>();

                for (int i = 0; i < drawCount; i++)
                {
                    GachaDraw.Result r;
                    if (isNormalBanner)
                    {
                        r = GachaDraw.DrawNormal(
                            characterPoolTableID, aWeaponPoolTableID, bWeaponPoolTableID,
                            characterRecords,
                            ref userData.NormalPityCount,
                            ref userData.NormalWeaponStack);
                    }
                    else
                    {
                        r = GachaDraw.DrawLimited(
                            characterPoolTableID, aWeaponPoolTableID, bWeaponPoolTableID,
                            GachaBannerTable.Banner.PickupCharacterID,
                            characterRecords,
                            ref userData.LimitedPityCount,
                            ref userData.IsLimitedGuaranteed,
                            ref userData.LimitedWeaponStack);
                    }

                    if (r.MileageAmount > 0)
                    {
                        userData.Balances[(int)CurrencyKey.Mileage] =
                            userData.Balances.GetValueOrDefault((int)CurrencyKey.Mileage) + r.MileageAmount;
                    }

                    if (r.Type == "item")
                        weaponRewards.Add(new TransactionItemSpec { ID = r.ID, Amount = 1 });

                    pullResults.Add(r);
                }

                var charParam = new Param();
                charParam.Add("characters", characterRecords);
                WriteBatcher.EnqueueWrite("OwnedCharacterData", charParam);

                if (weaponRewards.Count > 0)
                {
                    var rewardResult = RewardService.GiveReward(
                        weaponRewards, RewardFullPolicy.Overflow, inventoryConfigTableID, userData);

                    var instanceIDs = new List<long>(rewardResult.EquipItemDic.Keys);
                    int idx = 0;
                    for (int i = 0; i < pullResults.Count; i++)
                    {
                        if (pullResults[i].Type != "item") continue;
                        var r = pullResults[i];
                        r.InstanceID = instanceIDs[idx++];
                        pullResults[i] = r;
                    }
                }
                else
                {
                    var userParam = new Param();
                    userParam.Add("userData", userData);
                    WriteBatcher.EnqueueWrite("UserData", userParam);
                    var writeResult = WriteBatcher.Flush();
                    if (!string.IsNullOrEmpty(writeResult))
                        return ReturnObject.Error(writeResult);
                }

                var response = new GachaResponse
                {
                    PullResults = pullResults,
                    PityCount = isNormalBanner ? userData.NormalPityCount : userData.LimitedPityCount,
                    WeaponStack = isNormalBanner ? userData.NormalWeaponStack : userData.LimitedWeaponStack
                };
                return ReturnObject.Success(JsonConvert.SerializeObject(response));
            }
            catch (RewardException re)
            {
                return ReturnObject.Error(re.Code, re.Detail);
            }
            catch (Exception e)
            {
                return ReturnObject.Error(8000, e.Message);
            }
        }
    }
}
