using Amazon.Lambda.Core;
using BackEnd;
using Newtonsoft.Json;
using System;
using System.IO;
using BackendSharedLib;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BackendFunction
{
    /// <summary>
    /// stamina는 클라에서 저장, 재화만 검증
    /// </summary>
    public class BFunc
    {
        public Stream Function(Stream stream, ILambdaContext context)
        {
            try
            {
                Backend.Initialize(ref stream);

                string tableID = Backend.Content["tableID"].ToString();

                var tableResult = StaminaConfigTable.Set(tableID);
                if (!string.IsNullOrEmpty(tableResult))
                    return ReturnObject.Error(8000, tableResult);

                var bro = Backend.GameData.GetMyData("UserData", new Where());
                if (!bro.IsSuccess())
                    return ReturnObject.Error($"{bro.ErrorCode} : {bro.Message}");

                var userJson = bro.FlattenRows()[0]["userData"];
                var userData = JsonConvert.DeserializeObject<UserData>(userJson.ToJson());
                var config = StaminaConfigTable.Config;

                int gemID = (int)CurrencyKey.GEM;
                if (!userData.Balances.TryGetValue(gemID, out int ownedGem) || ownedGem < config.GemRechargeCost)
                {
                    var sParam = new Param();
                    sParam.Add("msg", $"Not enough gems. Owned: {ownedGem}, Required: {config.GemRechargeCost}");
                    return ReturnObject.Suspect(sParam);
                }

                userData.Balances[gemID] = ownedGem - config.GemRechargeCost;

                var userParam = new Param();
                userParam.Add("userData", userData);
                bro = Backend.GameData.Update("UserData", new Where(), userParam);
                if (!bro.IsSuccess())
                    return ReturnObject.Error($"{bro.ErrorCode} : {bro.Message}");

                return ReturnObject.Success();
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
