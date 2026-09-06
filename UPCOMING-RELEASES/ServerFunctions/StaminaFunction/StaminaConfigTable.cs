using BackEnd;
namespace BackendFunction
{
    public class StaminaConfig
    {
        public int MaxStamina;
        public int RecoveryIntervalSeconds;
        public int GemRechargeCost;
        public int GemRechargeAmount;
        public int MaxDailyGemRechargeCount;
    }



    public static class StaminaConfigTable
    {
        public static StaminaConfig Config;
        private static string cachedTableID;

        public static string Set(string tableID)
        {
            if (cachedTableID == tableID)
                return "";

            var bro = Backend.Chart.GetChartContents(tableID);
            if (!bro.IsSuccess())
                return $"{bro.ErrorCode} : {bro.Message}";

            var jsonData = bro.FlattenRows();
            for (int i = 0; i < jsonData.Count; i++)
            {
                var entry = new StaminaConfig()
                {
                    MaxStamina = int.Parse(jsonData[i]["MaxStamina"].ToString()),
                    RecoveryIntervalSeconds = int.Parse(jsonData[i]["RecoveryIntervalSeconds"].ToString()),
                    GemRechargeCost = int.Parse(jsonData[i]["GemRechargeCost"].ToString()),
                    GemRechargeAmount = int.Parse(jsonData[i]["GemRechargeAmount"].ToString()),
                    MaxDailyGemRechargeCount = int.Parse(jsonData[i]["MaxDailyGemRechargeCount"].ToString())
                };
                Config = entry;
            }
            cachedTableID = tableID;
            return "";
        }
    }
}