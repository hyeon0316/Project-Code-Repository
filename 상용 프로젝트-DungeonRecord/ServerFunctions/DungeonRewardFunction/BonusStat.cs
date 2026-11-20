namespace BackendFunction
{
    public class BonusStat
    {
        public string Type;
        /// <summary>
        /// upgradeCount
        /// </summary>
        public int Value;

        public BonusStat(string type)
        {
            Type = type;
            Value = 0;
        }
    }
}
