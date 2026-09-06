using BackEnd;
using BackendSharedLib;
using System;
using System.Collections.Generic;

namespace BackendFunction
{
    public static class GachaDraw
    {
        public struct Result
        {
            public string Type;
            public int ID;
            public long InstanceID;
            public int MileageAmount;
        }

        private const int MaxDupeToken = 6;

        private static readonly Random rand = new();

        public static Result DrawNormal(
            string characterPoolTableID,
            string aWeaponPoolTableID,
            string bWeaponPoolTableID,
            List<CharacterRecord> characterRecords,
            ref int pityCount,
            ref int weaponStack)
        {
            int charType = DrawCharacterNormal(characterPoolTableID, ref pityCount);
            return ResolvePull(charType, characterRecords, aWeaponPoolTableID, bWeaponPoolTableID, ref weaponStack);
        }

        public static Result DrawLimited(
            string characterPoolTableID,
            string aWeaponPoolTableID,
            string bWeaponPoolTableID,
            string pickupCharacterID,
            List<CharacterRecord> characterRecords,
            ref int pityCount,
            ref bool isLimitedGuaranteed,
            ref int weaponStack)
        {
            int charType = DrawCharacterLimited(characterPoolTableID, pickupCharacterID, ref pityCount, ref isLimitedGuaranteed);
            return ResolvePull(charType, characterRecords, aWeaponPoolTableID, bWeaponPoolTableID, ref weaponStack);
        }

        private static Result ResolvePull(
            int charType,
            List<CharacterRecord> characterRecords,
            string aWeaponPoolTableID,
            string bWeaponPoolTableID,
            ref int weaponStack)
        {
            bool isPityHit = weaponStack + 1 >= GachaRuleTable.Rule.WeaponPityMax;

            if (charType != 0)
            {
                int mileage = 0;
                var existing = characterRecords.Find(c => c.Type == charType);
                if (existing == null)
                    characterRecords.Add(new CharacterRecord { Type = charType });
                else if (existing.DupeTokenOwned < MaxDupeToken)
                {
                    existing.DupeTokenOwned++;
                    mileage = GachaRuleTable.Rule.DupeMileageAmount;
                }
                else
                {
                    mileage = GachaRuleTable.Rule.OverflowMileageAmount;
                }

                if (isPityHit) weaponStack = 0;

                return new Result { Type = "character", ID = charType, MileageAmount = mileage };
            }

            if (isPityHit || rand.NextDouble() < GachaRuleTable.Rule.AWeaponRate)
            {
                int aID = DrawWeapon(aWeaponPoolTableID);
                weaponStack = 0;
                return new Result
                {
                    Type = "item",
                    ID = aID,
                    MileageAmount = GachaRuleTable.Rule.AWeaponMileageAmount
                };
            }

            int bID = DrawWeapon(bWeaponPoolTableID);
            weaponStack++;
            return new Result { Type = "item", ID = bID, MileageAmount = 0 };
        }

        private static int DrawCharacterNormal(string poolTableID, ref int pityCount)
        {
            float rate = ComputeRate(pityCount);
            pityCount++;

            if ((float)rand.NextDouble() > rate)
                return 0;

            var bro = Backend.Probability.GetProbabilitys(poolTableID, 1);
            if (!bro.IsSuccess())
                throw new Exception($"GetProbabilitys failed: {bro.ErrorCode}");

            int charType = int.Parse(bro.GetFlattenJSON()["elements"][0]["CharacterID"].ToString());
            pityCount = 0;
            return charType;
        }

        private static int DrawCharacterLimited(string poolTableID, string pickupCharacterID, ref int pityCount, ref bool isLimitedGuaranteed)
        {
            float rate = ComputeRate(pityCount);
            pityCount++;

            if ((float)rand.NextDouble() > rate)
                return 0;

            pityCount = 0;

            if (isLimitedGuaranteed)
            {
                isLimitedGuaranteed = false;
                return int.Parse(pickupCharacterID);
            }

            if ((float)rand.NextDouble() <= GachaRuleTable.Rule.OffBannerRate)
            {
                var bro = Backend.Probability.GetProbabilitys(poolTableID, 1);
                if (!bro.IsSuccess())
                    throw new Exception($"GetProbabilitys failed: {bro.ErrorCode}");

                int drawn = int.Parse(bro.GetFlattenJSON()["elements"][0]["CharacterID"].ToString());
                isLimitedGuaranteed = true;
                return drawn;
            }

            return int.Parse(pickupCharacterID);
        }

        private static int DrawWeapon(string weaponPoolTableID)
        {
            var bro = Backend.Probability.GetProbabilitys(weaponPoolTableID, 1);
            if (!bro.IsSuccess())
                throw new Exception($"GetProbabilitys failed: {bro.ErrorCode}");
            return int.Parse(bro.GetFlattenJSON()["elements"][0]["ItemID"].ToString());
        }

        private static float ComputeRate(int pityCount)
        {
            var rule = GachaRuleTable.Rule;
            int currentPull = pityCount + 1;
            if (currentPull >= rule.MaxPity)
                return 1f;
            if (currentPull < rule.SoftPityStartCount)
                return rule.BaseRate;
            return rule.BaseRate + (currentPull - rule.SoftPityStartCount) * rule.RateIncreasePerCount;
        }
    }
}
