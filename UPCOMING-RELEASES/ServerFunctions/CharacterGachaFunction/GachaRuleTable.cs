using BackEnd;
namespace BackendFunction
{
    public class GachaRule
    {
        public int MaxPity;
        public float BaseRate;
        public int SoftPityStartCount;
        public float RateIncreasePerCount;
        public float OffBannerRate;
        public int DupeMileageAmount;
        public int OverflowMileageAmount;
        public float AWeaponRate;
        public int AWeaponMileageAmount;
        public int WeaponPityMax;
    }



    public static class GachaRuleTable
    {
        public static GachaRule Rule;
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
                var entry = new GachaRule()
                {
                    MaxPity = int.Parse(jsonData[i]["MaxPity"].ToString()),
                    BaseRate = float.Parse(jsonData[i]["BaseRate"].ToString()),
                    SoftPityStartCount = int.Parse(jsonData[i]["SoftPityStartCount"].ToString()),
                    RateIncreasePerCount = float.Parse(jsonData[i]["RateIncreasePerCount"].ToString()),
                    OffBannerRate = float.Parse(jsonData[i]["OffBannerRate"].ToString()),
                    DupeMileageAmount = int.Parse(jsonData[i]["DupeMileageAmount"].ToString()),
                    OverflowMileageAmount = int.Parse(jsonData[i]["OverflowMileageAmount"].ToString()),
                    AWeaponRate = float.Parse(jsonData[i]["AWeaponRate"].ToString()),
                    AWeaponMileageAmount = int.Parse(jsonData[i]["AWeaponMileageAmount"].ToString()),
                    WeaponPityMax = int.Parse(jsonData[i]["WeaponPityMax"].ToString()),
                };
                Rule = entry;
            }
            cachedTableID = tableID;
            return "";
        }
    }
}
