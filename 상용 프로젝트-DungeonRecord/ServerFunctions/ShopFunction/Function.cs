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

                var shopTableID = Backend.Content["shopTableID"].ToString();
                var inventoryConfigTableID = Backend.Content["inventoryConfigTableID"].ToString();

                var tableResult = ShopTable.Set(shopTableID);
                if (!string.IsNullOrEmpty(tableResult))
                    return ReturnObject.Error(tableResult);

                if (Backend.HasKey("token"))
                    return RealPurchase.Run(inventoryConfigTableID);

                return VirtualPurchase.Run(inventoryConfigTableID);
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
