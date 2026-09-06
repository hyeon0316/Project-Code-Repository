using BackEnd;
using LitJson;

public static class BFuncResponseHandler
{
    public static JsonData Parse(BackendReturnObject bro, string context = "BFunc")
    {
        if (bro == null)
        {
            ErrorReporter.Report(EErrorCode.ServerUnknown, context, "bro is null");
            return null;
        }

        if (!bro.IsSuccess())
        {
            EErrorCode code = bro.IsMaintenanceError() ? EErrorCode.ServerMaintenance : EErrorCode.BFuncInvokeFailed;
            ErrorReporter.Report(code, context, bro.ToString());
            return null;
        }

        var jsonData = bro.GetReturnValuetoJSON()["result"].ToString();
        var innerJsonData = JsonMapper.ToObject(jsonData);
        if (innerJsonData.ContainsKey("suspect"))
        {
            ErrorReporter.Report(EErrorCode.BFuncSuspect, context, innerJsonData["suspect"].ToString());
            return null;
        }
        else if (innerJsonData.ContainsKey("error"))
        {
            ReportError(innerJsonData["error"], context);
            return null;
        }

        var success = innerJsonData["success"];
        WriteSuccessLog(success, context);
        return success;
    }

    /// <summary>
    /// 성공 응답의 단일 로그 지점. 호출부마다 로그를 붙이면 누락되거나
    /// 실패 분기에서 응답을 참조하는 실수가 생긴다.
    /// HDebug.Log는 릴리즈 빌드에서 제거되므로 직렬화 비용도 남지 않는다.
    /// </summary>
    private static void WriteSuccessLog(JsonData success, string context)
    {
        HDebug.Log($"[BFunc] ctx={context} | {(success != null ? success.ToString() : "null")}");
    }

    /// <summary>
    /// error는 두 형태를 모두 받는다.
    /// 구형: "임의의 문자열"  /  신형: { "code": 6002, "detail": "..." }
    /// 서버와 클라의 배포 시점이 다르고 구버전 빌드가 스토어에 남아 있어 양쪽을 지원해야 한다.
    /// 어느 쪽이든 원문은 로그 전용이며 사용자 채널에 도달하지 않는다.
    /// </summary>
    private static void ReportError(JsonData errorNode, string context)
    {
        EErrorCode code = EErrorCode.BFuncServerError;
        string detail = errorNode.ToString();

        if (errorNode.IsObject && errorNode.ContainsKey("code")
            && int.TryParse(errorNode["code"].ToString(), out int serverCode))
        {
            code = ErrorCodeMap.FromServerCode(serverCode);
            detail = errorNode.ContainsKey("detail") ? errorNode["detail"].ToString() : string.Empty;
        }

        ErrorReporter.Report(code, context, detail);
    }
}
