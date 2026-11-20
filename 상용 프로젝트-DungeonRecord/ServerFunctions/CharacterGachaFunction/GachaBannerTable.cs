using BackEnd;
using System;
using System.Globalization;
namespace BackendFunction
{
    public class GachaBanner
    {
        public string BannerID;
        public DateTime StartDate;
        public DateTime EndDate;
        public string PickupCharacterID;
    }



    public static class GachaBannerTable
    {
        public static GachaBanner Banner;
        private static string cachedTableID;

        public static string Set(string tableID)
        {
            if (cachedTableID == tableID)
                return "";

            var bro = Backend.Chart.GetChartContents(tableID);
            if (!bro.IsSuccess())
                return $"{bro.ErrorCode} : {bro.Message}";

            var jsonData = bro.FlattenRows();
            Banner = new GachaBanner
            {
                BannerID = jsonData[0]["BannerID"].ToString(),
                StartDate = DateTime.Parse(
                    jsonData[0]["StartDate"].ToString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal),
                EndDate = DateTime.Parse(
                    jsonData[0]["EndDate"].ToString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal),
                PickupCharacterID = jsonData[0]["PickupCharacterID"].ToString()
            };

            cachedTableID = tableID;
            return "";
        }
    }
}
