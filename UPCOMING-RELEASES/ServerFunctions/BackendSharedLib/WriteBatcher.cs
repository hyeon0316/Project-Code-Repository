using BackEnd;
using System.Collections.Generic;

namespace BackendSharedLib
{
    public static class WriteBatcher
    {
        private static List<(string tableName, Param param)> pendingWrites = new();
        private static readonly int maxBatchSize = 10;

        public static void EnqueueWrite(string tableName, Param param)
        {
            pendingWrites.Add((tableName, param));
            if (pendingWrites.Count >= maxBatchSize)
                Flush();
        }

        public static string Flush()
        {
            if (pendingWrites.Count == 0)
                return "";

            var writeTransactions = new List<TransactionValue>();
            foreach (var pair in pendingWrites)
                writeTransactions.Add(TransactionValue.SetUpdate(pair.tableName, new Where(), pair.param));

            if (writeTransactions.Count > 1)
            {
                var bro = Backend.GameData.TransactionWriteV2(writeTransactions);
                if (!bro.IsSuccess())
                    return $"[WriteBatcher_Tracsaction]{bro.ErrorCode}: {bro.Message}";
            }
            else
            {
                var bro = Backend.GameData.Update(pendingWrites[0].tableName, new Where(), pendingWrites[0].param);
                if (!bro.IsSuccess())
                    return $"[WriteBatcher]{bro.ErrorCode}: {bro.Message}";
            }
            pendingWrites.Clear();
            return "";
        }
    }
}