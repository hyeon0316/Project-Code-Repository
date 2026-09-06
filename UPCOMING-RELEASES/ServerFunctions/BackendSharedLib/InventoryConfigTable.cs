using System.Collections.Generic;
using BackEnd;

namespace BackendSharedLib
{
    public class InventoryConfigEntry
    {
        public string ConfigID = "";
        public int Value;
    }

    public static class InventoryConfigTable
    {
        public static List<InventoryConfigEntry> Entries = new();
        private static string cacheChartID = "";

        public static string Set(string chartID)
        {
            if (cacheChartID == chartID && Entries.Count != 0)
                return string.Empty;

            Entries.Clear();

            var bro = Backend.Chart.GetChartContents(chartID);
            if (!bro.IsSuccess())
                return $"[InventoryConfigTable]{bro.ErrorCode}: {bro.Message}";

            var rows = bro.FlattenRows();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var entry = new InventoryConfigEntry
                {
                    ConfigID = row["ConfigID"].ToString(),
                    Value = int.Parse(row["Value"].ToString())
                };
                Entries.Add(entry);
            }

            cacheChartID = chartID;
            return string.Empty;
        }

        public static int GetValue(string id)
        {
            foreach (var entry in Entries)
            {
                if (entry.ConfigID == id)
                    return entry.Value;
            }
            return 0;
        }
    }
}