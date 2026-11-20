using BackEnd;
using Cysharp.Threading.Tasks;

public static class BFuncInvoker
{
    public const int RETRY_MAX = 3;
    private const int RETRY_DELAY_MS = 5000;

    public static async UniTask<BackendReturnObject> InvokeAsync(string function, Param param, int retryMax = 0)
    {
        if (retryMax > 0)
            param.Add("requestID", System.Guid.NewGuid().ToString("N"));

        var bro = await InvokeOnce(function, param);
        for (int retry = 0; retry < retryMax && BFuncResponseHandler.IsRetryable(bro); retry++)
        {
            await UniTask.Delay(RETRY_DELAY_MS, ignoreTimeScale: true);
            bro = await InvokeOnce(function, param);
        }
        return bro;
    }

    private static async UniTask<BackendReturnObject> InvokeOnce(string function, Param param)
    {
        BackendReturnObject bro = null;
        SendQueue.Enqueue(Backend.BFunc.InvokeFunction, function, param, callback =>
        {
            bro = callback;
        });
        await UniTask.WaitUntil(() => bro != null);
        return bro;
    }
}
