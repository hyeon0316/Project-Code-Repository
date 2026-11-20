using BackEnd;
using System.Collections.Generic;
using System.Linq;

namespace BackendSharedLib
{
    public static class PendingGrantService
    {
        public static string Record(UserData userData, PendingGrant grant)
        {
            userData.PendingGrants.Add(grant);
            var userParam = new Param();
            userParam.Add("userData", userData);
            var bro = Backend.GameData.Update("UserData", new Where(), userParam);
            return bro.IsSuccess() ? string.Empty : $"{bro.ErrorCode}: {bro.Message}";
        }

        public static TransactionResult Grant(UserData userData, List<PendingGrant> grants, string inventoryConfigTableID)
        {
            var items = grants.SelectMany(g => g.Items).ToList();
            foreach (var grant in grants)
                userData.PendingGrants.Remove(grant);

            var result = RewardService.GiveReward(items, RewardFullPolicy.Overflow, inventoryConfigTableID, userData, flush: false);

            var userParam = new Param();
            userParam.Add("userData", userData);
            WriteBatcher.ReplaceWrite("UserData", userParam);
            var writeResult = WriteBatcher.Flush();
            if (!string.IsNullOrEmpty(writeResult))
                throw new RewardException(8000, writeResult);

            return result;
        }
    }
}
