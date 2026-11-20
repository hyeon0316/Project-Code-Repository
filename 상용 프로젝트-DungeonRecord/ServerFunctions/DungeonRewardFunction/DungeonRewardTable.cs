using BackEnd;
using System.Collections.Generic;

namespace BackendFunction
{
    public class DungeonRewardEntry
    {
        public int ItemID;
        public int Min;
        public int Max;
    }

    public static class DungeonRewardTable
    {
        public static Dictionary<string, List<DungeonRewardEntry>> Entries = new();
        private static string cacheChartID = "";

        public static string Set(string chartID)
        {
            if (cacheChartID == chartID && Entries.Count != 0)
                return string.Empty;

            Entries.Clear();

            var bro = Backend.Chart.GetChartContents(chartID);
            if (!bro.IsSuccess())
                return $"[DungeonRewardTable]{bro.ErrorCode}: {bro.Message}";

            var rows = bro.FlattenRows();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                string rewardID = row["RewardID"].ToString();
                var entry = new DungeonRewardEntry
                {
                    ItemID = int.Parse(row["ItemID"].ToString()),
                    Min = int.Parse(row["Min"].ToString()),
                    Max = int.Parse(row["Max"].ToString())
                };
                if (!Entries.ContainsKey(rewardID))
                    Entries.Add(rewardID, new List<DungeonRewardEntry>());
                Entries[rewardID].Add(entry);
            }

            cacheChartID = chartID;
            return string.Empty;
        }
    }
}
