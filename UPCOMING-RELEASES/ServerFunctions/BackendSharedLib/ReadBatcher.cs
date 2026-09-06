using BackEnd;
using LitJson;
using System.Collections.Generic;

namespace BackendSharedLib
{
    public static class ReadBatcher
    {
        private static List<string> pendingReads = new();

        public static void EnqueueRead(string tableName)
        {
            pendingReads.Add(tableName);
        }

        public static JsonData Flush()
        {
            var result = new JsonData();

            var readTransactions = new List<TransactionValue>();
            foreach (var tableName in pendingReads)
                readTransactions.Add(TransactionValue.SetGet(tableName, new Where()));

            if (readTransactions.Count > 1)
            {
                var bro = Backend.GameData.TransactionReadV2(readTransactions);
                if (!bro.IsSuccess())
                {
                    result["error"] = $"{bro.ErrorCode}: {bro.ErrorMessage}";
                    return result;
                }
                var gameDataListJson = bro.GetFlattenJSON()["Responses"];
                for (int i = 0; i < pendingReads.Count; i++)
                {
                    result[pendingReads[i]] = gameDataListJson[i];
                }
            }
            else
            {
                var bro = Backend.GameData.GetMyData(pendingReads[0], new Where());
                if (!bro.IsSuccess())
                {
                    result["error"] = $"{bro.ErrorCode}: {bro.ErrorMessage}";
                    return result;
                }
                result[pendingReads[0]] = bro.FlattenRows()[0];
            }

            pendingReads.Clear();
            return result;
        }
    }
}