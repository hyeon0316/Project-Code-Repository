using Amazon.Lambda.Core;
using BackEnd;
using System;
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

                var rewardID = Backend.Content["rewardID"].ToString();
                var rewardTableID = Backend.Content["RewardTableID"].ToString();
                var inventoryConfigTableID = Backend.Content["inventoryConfigTableID"].ToString();

                return Backend.HasKey("isWearable")
                    ? WearableGiver.Give(rewardID, rewardTableID, inventoryConfigTableID)
                    : StackableGiver.Give(rewardID, rewardTableID, inventoryConfigTableID);
            }
            catch (RewardException re)
            {
                return ReturnObject.Error(re.Code, re.Detail);
            }
            catch (Exception e)
            {
                return ReturnObject.Error(e.Message);
            }
        }
    }
}
