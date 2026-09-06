namespace BackendSharedLib
{
    public static class ItemID
    {
        public static int GetType(int id) => id / 10000;
        public static int GetRarity(int id) => (id / 1000) % 10;
        public static int GetSubType(int id) => (id / 100) % 10;
        public static int GetIndex(int id) => id % 100;

        public static bool IsCurrency(int id) => GetType(id) == 1;
        public static bool IsWearable(int id) => GetType(id) == 2;
        public static bool IsWeapon(int id) => GetType(id) == 3;
        public static bool IsStackable(int id) => GetType(id) == 4;
        public static bool IsExp(int id) => GetType(id) == 6;

        // 장비(Type=2) SubType → 타입 문자열
        public static string GetEquipSubTypeName(int id)
        {
            return GetSubType(id) switch
            {
                0 => "HeadGear",
                1 => "Armor",
                2 => "Boots",
                3 => "Accessory",
                _ => "Unknown"
            };
        }

        // Rarity 숫자 → bool (A 이상이면 true)
        public static bool IsRarityLocked(int id) => GetRarity(id) >= 1;
    }
}
