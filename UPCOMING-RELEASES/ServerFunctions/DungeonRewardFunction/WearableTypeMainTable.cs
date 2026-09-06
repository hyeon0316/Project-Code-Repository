using BackEnd;
using System.Collections.Generic;

namespace BackendFunction
{
    public static class WearableTypeMainTable
    {
        public static Dictionary<string, List<string>> Entries = new();
        private static string cacheChartID = "";

        public static string Set(string chartID)
        {
            if (cacheChartID == chartID && Entries.Count != 0)
                return string.Empty;

            Entries.Clear();

            var bro = Backend.Chart.GetChartContents(chartID);
            if (!bro.IsSuccess())
                return bro.ErrorCode;

            var rows = bro.FlattenRows();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                string type = row["Type"].ToString();
                string mainStat = row["MainStat"].ToString();
                if (!Entries.ContainsKey(type))
                    Entries.Add(type, new List<string>());

                Entries[type].Add(mainStat);
            }

            cacheChartID = chartID;
            return string.Empty;
        }


    }
}