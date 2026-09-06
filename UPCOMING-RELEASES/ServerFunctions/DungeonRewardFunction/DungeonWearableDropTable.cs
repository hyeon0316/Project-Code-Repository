using BackEnd;
using System.Collections.Generic;

namespace BackendFunction
{
    public class DungeonWearableDropEntry
    {
        public int Rarity;
        public int Min;
        public int Max;
    }

    public static class DungeonWearableDropTable
    {
        public static Dictionary<int, List<DungeonWearableDropEntry>> Entries = new();
        private static string cacheChartID = "";

        public static string Set(string chartID)
        {
            if (cacheChartID == chartID && Entries.Count != 0)
                return string.Empty;

            Entries.Clear();

            var bro = Backend.Chart.GetChartContents(chartID);
            if (!bro.IsSuccess())
                return $"[DungeonWearableDropTable]{bro.ErrorCode}: {bro.Message}";

            var rows = bro.FlattenRows();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                int worldModPhase = int.Parse(row["WorldModPhase"].ToString());
                var entry = new DungeonWearableDropEntry
                {
                    Rarity = ParseRarity(row["Rarity"].ToString()),
                    Min = int.Parse(row["Min"].ToString()),
                    Max = int.Parse(row["Max"].ToString())
                };
                if (!Entries.ContainsKey(worldModPhase))
                    Entries.Add(worldModPhase, new List<DungeonWearableDropEntry>());
                Entries[worldModPhase].Add(entry);
            }

            cacheChartID = chartID;
            return string.Empty;
        }

        private static int ParseRarity(string s)
        {
            switch (s)
            {
                case "B": return 0;
                case "A": return 1;
                case "S": return 2;
                default: return 0;
            }
        }
    }
}
