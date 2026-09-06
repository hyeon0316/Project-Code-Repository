using System.Collections.Generic;

namespace BackendSharedLib
{
    public class UserData
    {
        public long TotalExp;
        public int LastReceivedRewardLevel;
        public Dictionary<int, int> Balances = new();
        public int StartingCharacter;
        public HashSet<string> ClaimedDungeonRewardIDs = new();
        public int NormalPityCount;
        public int LimitedPityCount;
        public bool IsLimitedGuaranteed;
        public int NormalWeaponStack;
        public int LimitedWeaponStack;
    }
}
