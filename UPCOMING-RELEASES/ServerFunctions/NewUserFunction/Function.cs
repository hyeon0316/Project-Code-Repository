using Amazon.Lambda.Core;
using BackEnd;
using LitJson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

                var uid = Backend.Content["uid"].ToString();
                var nickName = Backend.Content["nickName"].ToString();
                var characterType = int.Parse(Backend.Content["characterType"].ToString());
                var shopTableID = Backend.Content["shopTableID"].ToString();
                var battlePassSeasonID = Backend.HasKey("seasonID") ? Backend.Content["seasonID"].ToString() : "";
                if (characterType != 1 && characterType != 2)
                {
                    var param = new Param();
                    param.Add("msg", $"{characterType}, 기본 캐릭터 외 다른 캐릭터 설정");
                    return ReturnObject.Suspect(param);
                }

                var bro1 = Backend.GameData.Insert("UserData", UserDataParam.Get(characterType));
                if (!bro1.IsSuccess())
                    return ReturnObject.Error(8000, $"UserData: {bro1.ErrorCode}");

                var writeTransactions = new List<TransactionValue>();
                writeTransactions.Add(TransactionValue.SetInsert("QuestData", QuestParam.Get()));
                writeTransactions.Add(TransactionValue.SetInsert("UserProfile", UserProfileParam.Get(nickName, uid)));
                writeTransactions.Add(TransactionValue.SetInsert("OwnedCharacterData", OwnedCharacterDataParam.Get(characterType)));
                writeTransactions.Add(TransactionValue.SetInsert("ShopRecordData", ShopParam.Get(shopTableID)));
                writeTransactions.Add(TransactionValue.SetInsert("Inventory_Stack", Inventory_StackParam.Get()));
                writeTransactions.Add(TransactionValue.SetInsert("Inventory_Equip", Inventory_EquipParam.Get()));
                writeTransactions.Add(TransactionValue.SetInsert("AdventureGuideData", AdventureGuideParam.Get()));
                writeTransactions.Add(TransactionValue.SetInsert("BattlePassData", BattlePassParam.Get(battlePassSeasonID)));
                writeTransactions.Add(TransactionValue.SetInsert("AchievementData", AchievementParam.Get()));

                var bro2 = Backend.GameData.TransactionWriteV2(writeTransactions);
                if (!bro2.IsSuccess())
                    return ReturnObject.Error(8000, $"{bro2.ErrorCode} : {bro2.Message}");

                return ReturnObject.Success(bro2.ToString());
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
