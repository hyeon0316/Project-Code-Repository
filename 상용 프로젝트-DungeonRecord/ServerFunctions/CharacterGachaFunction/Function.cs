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
                if (WarmUp.IsRequest())
                    return ReturnObject.Success();
                ReadBatcher.Clear();
                WriteBatcher.Clear();

                var ruleTableID = Backend.Content["ruleTableID"].ToString();
                var bannerTableID = Backend.Content["bannerTableID"].ToString();
                var aWeaponPoolTableID = Backend.Content["aWeaponPoolTableID"].ToString();
                var bWeaponPoolTableID = Backend.Content["bWeaponPoolTableID"].ToString();
                var inventoryConfigTableID = Backend.Content["inventoryConfigTableID"].ToString();
                var isNormalBanner = Backend.HasKey("isNormal");
                int drawCount = Backend.HasKey("isTenPull") ? 10 : 1;

                var tableResult = GachaRuleTable.Set(ruleTableID);
                if (!string.IsNullOrEmpty(tableResult))
                    return ReturnObject.Error(tableResult);

                var configResult = InventoryConfigTable.Set(inventoryConfigTableID);
                if (!string.IsNullOrEmpty(configResult))
                    return ReturnObject.Error(configResult);

                ReadBatcher.EnqueueRead("UserData");
                ReadBatcher.EnqueueRead("OwnedCharacterData");
                ReadBatcher.EnqueueRead("Inventory_Equip");
                var readResult = ReadBatcher.Flush();
                if (readResult.ContainsKey("error"))
                    return ReturnObject.Error(readResult["error"].ToString());

                var userData = JsonConvert.DeserializeObject<UserData>(readResult["UserData"]["userData"].ToJson());
                if (RequestGuard.TryReplay(userData, out var replay))
                    return replay;
                var characterRecords = JsonConvert.DeserializeObject<List<CharacterRecord>>(readResult["OwnedCharacterData"]["characters"].ToJson());

                int ownedWeaponCount = readResult["Inventory_Equip"]["weapons"].Count;
                if (ownedWeaponCount + drawCount > InventoryConfigTable.GetValue("Weapon"))
                    return ReturnObject.Error(6005, "InventoryFull: Not enough weapon slot");

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

                var bannerResult = GachaBannerTable.Set(bannerTableID);
                if (!string.IsNullOrEmpty(bannerResult))
                    return ReturnObject.Error(bannerResult);

                GachaBanner banner = null;
                if (!isNormalBanner)
                {
                    var bannerID = Backend.HasKey("bannerID") ? Backend.Content["bannerID"].ToString() : "";
                    banner = GachaBannerTable.FindLimited(bannerID);
                    if (banner == null)
                    {
                        var sp = new Param();
                        sp.Add("msg", "존재하지 않는 한정 배너");
                        sp.Add("bannerID", bannerID);
                        return ReturnObject.Suspect(sp);
                    }
                    if (!banner.IsOpen(DateTime.UtcNow))
                        return ReturnObject.Error(6003, $"Banner {bannerID} is not open.");
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
                            aWeaponPoolTableID, bWeaponPoolTableID,
                            characterRecords,
                            ref userData.NormalPityCount,
                            ref userData.NormalWeaponStack);
                    }
                    else
                    {
                        r = GachaDraw.DrawLimited(
                            aWeaponPoolTableID, bWeaponPoolTableID,
                            banner,
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
                        weaponRewards, RewardFullPolicy.Overflow, inventoryConfigTableID, userData, flush: false);

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

                var response = new GachaResponse
                {
                    PullResults = pullResults,
                    PityCount = isNormalBanner ? userData.NormalPityCount : userData.LimitedPityCount,
                    WeaponStack = isNormalBanner ? userData.NormalWeaponStack : userData.LimitedWeaponStack
                };
                var responseJson = JsonConvert.SerializeObject(response);
                var writeResult = RequestGuard.Commit(userData, responseJson);
                if (!string.IsNullOrEmpty(writeResult))
                    return ReturnObject.Error(writeResult);
                return ReturnObject.Success(responseJson);
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
