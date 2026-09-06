public static class ItemID
{
    public static int GetType(int id) { return id / 10000; }
    public static int GetRarity(int id) { return (id / 1000) % 10; }
    public static int GetSubType(int id) { return (id / 100) % 10; }
    public static int GetIndex(int id) { return id % 100; }

    public static bool IsCurrency(int id) { return GetType(id) == 1; }
    public static bool IsWearable(int id) { return GetType(id) == 2; }
    public static bool IsWeapon(int id) { return GetType(id) == 3; }
    public static bool IsStackable(int id) { return GetType(id) == 4; }
    public static bool IsExp(int id) { return GetType(id) == 6; }

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

    public static bool IsRarityLocked(int id) { return GetRarity(id) >= 1; }

    /// <summary>등급 자릿수를 0으로 죽인 ID.</summary>
    public static int GetBaseID(int id) { return id - (GetRarity(id) * 1000); }
}
