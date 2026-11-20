using Amazon.Lambda.Core;
using BackEnd;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using BackendSharedLib;

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

                var mailResult = new MailReceiveResult();
                string inventoryConfigTableID = Backend.Content["inventoryConfigTableID"].ToString();
                int ownedWeaponCount = -1;

                string CheckWeaponSlot(PostType pt, string mailID)
                {
                    var listBro = Backend.UPost.GetPostList(pt, 100);
                    if (!listBro.IsSuccess())
                        return $"{listBro.ErrorCode}: {listBro.Message}";

                    var postList = listBro.GetReturnValuetoJSON()["postList"];
                    int weaponCount = 0;
                    for (int i = 0; i < postList.Count; i++)
                    {
                        if (mailID != null && postList[i]["inDate"].ToString() != mailID)
                            continue;

                        foreach (var spec in PostItemGetter.Get(postList[i]["items"]))
                        {
                            if (ItemID.IsWeapon(spec.ID))
                                weaponCount++;
                        }
                    }

                    if (weaponCount == 0)
                        return string.Empty;

                    if (ownedWeaponCount < 0)
                    {
                        var configResult = InventoryConfigTable.Set(inventoryConfigTableID);
                        if (!string.IsNullOrEmpty(configResult))
                            return configResult;

                        ReadBatcher.EnqueueRead("Inventory_Equip");
                        var readResult = ReadBatcher.Flush();
                        if (readResult.ContainsKey("error"))
                            return readResult["error"].ToString();
                        ownedWeaponCount = readResult["Inventory_Equip"]["weapons"].Count;
                    }

                    if (ownedWeaponCount + weaponCount > InventoryConfigTable.GetValue("Weapon"))
                        return "InventoryFull";
                    return string.Empty;
                }

                void ProcessPost(PostType pt, string mailID, Func<BackendReturnObject> receive, bool isAll)
                {
                    var postResult = new MailReceivePostResult();
                    mailResult.Results[pt.ToString()] = postResult;

                    var slotResult = CheckWeaponSlot(pt, mailID);
                    if (!string.IsNullOrEmpty(slotResult))
                    {
                        postResult.Success = false;
                        postResult.Log = slotResult;
                        return;
                    }

                    var bro = receive();
                    if (!bro.IsSuccess())
                    {
                        postResult.Success = false;
                        postResult.Log = $"{bro.ErrorCode}: {bro.Message}";
                        return;
                    }

                    PendingGrant grant;
                    UserData userData;
                    try
                    {
                        var json = bro.GetReturnValuetoJSON()["postItems"];
                        var items = isAll ? PostItemGetter.GetAll(json) : PostItemGetter.Get(json);
                        grant = new PendingGrant { ID = Guid.NewGuid().ToString("N"), Items = items };

                        var userBro = Backend.GameData.GetMyData("UserData", new Where());
                        if (!userBro.IsSuccess())
                            throw new Exception($"Get UserData failed: {userBro.ErrorCode}");
                        userData = JsonConvert.DeserializeObject<UserData>(userBro.FlattenRows()[0]["userData"].ToJson());

                        var recordResult = PendingGrantService.Record(userData, grant);
                        if (!string.IsNullOrEmpty(recordResult))
                            throw new Exception(recordResult);
                    }
                    catch (Exception e)
                    {
                        var logParam = new Param();
                        logParam.Add("postType", pt.ToString());
                        logParam.Add("log", e.Message);
                        Backend.GameLog.InsertLogV2("Mail Received Not Recorded", logParam);
                        postResult.Success = false;
                        postResult.Log = e.Message;
                        return;
                    }

                    try
                    {
                        var change = PendingGrantService.Grant(userData, new List<PendingGrant> { grant }, inventoryConfigTableID);
                        if (ownedWeaponCount >= 0)
                            ownedWeaponCount += change.EquipItemDic.Count;

                        postResult.Change = change;
                        postResult.Success = true;
                    }
                    catch (Exception e)
                    {
                        ReadBatcher.Clear();
                        WriteBatcher.Clear();
                        postResult.Success = false;
                        postResult.Log = e.Message;
                    }
                }

                if (!Backend.HasKey("mailID"))
                {
                    if (Backend.HasKey("hasAdmin"))
                        ProcessPost(PostType.Admin, null, () => Backend.UPost.ReceivePostItemAll(PostType.Admin), isAll: true);
                    if (Backend.HasKey("hasCoupon"))
                        ProcessPost(PostType.Coupon, null, () => Backend.UPost.ReceivePostItemAll(PostType.Coupon), isAll: true);
                }
                else
                {
                    string mailID = Backend.Content["mailID"].ToString();
                    var pt = Enum.Parse<PostType>(Backend.Content["postType"].ToString());
                    ProcessPost(pt, mailID, () => Backend.UPost.ReceivePostItem(pt, mailID), isAll: false);
                }

                return ReturnObject.Success(JsonConvert.SerializeObject(mailResult));
            }
            catch (Exception e)
            {
                return ReturnObject.Error(8001, e.Message);
            }
        }
    }
}
