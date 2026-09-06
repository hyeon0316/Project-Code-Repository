using BackEnd;
using BackEnd.BackndLitJson;
using System.Collections.Generic;
using System.IO;

namespace BackendSharedLib
{
    public static class ReturnObject
    {
        public static Stream Suspect(Param param)
        {
            Backend.GameLog.InsertLogV2("Potential Cheater", param);

            var suspect = new JsonData();
            suspect["suspect"] = "비정상 이용이 감지되어 이용이 제한됩니다.\n 자세한 내용은 ~~로 문의 바랍니다";
            return Backend.JsonToStream(suspect);
        }

        public static Stream Error(int code, string detail)
        {
            var error = new JsonData();
            var body = new JsonData();
            body["code"] = code;
            body["detail"] = detail ?? "";
            error["error"] = body;
            return Backend.JsonToStream(error);
        }

        public static Stream Error(string msg)
        {
            return Error(8000, msg);
        }

        public static Stream Success(string msg)
        {
            var success = new JsonData();
            success["success"] = msg;
            return Backend.JsonToStream(success);
        }

        public static Stream Success(int value)
        {
            var success = new JsonData();
            success["success"] = value;
            return Backend.JsonToStream(success);
        }

        public static Stream Success()
        {
            var success = new JsonData();
            success["success"] = true;
            return Backend.JsonToStream(success);
        }
    }
}