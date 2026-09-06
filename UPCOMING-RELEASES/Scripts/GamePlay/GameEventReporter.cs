using System;

/// <summary>
/// 게임플레이 이벤트 신호 소스. 각 발생 지점에서 Report를 호출한다.
/// ProgressSignalRouter가 구독해 IProgressTracker로 라우팅한다.
/// </summary>
public static class GameEventReporter
{
    /// <summary>(신호 타입, 대상 ID, 값). id가 빈 문자열이면 대상 무관.</summary>
    public static event Action<EGameEventType, string, int> OnReported;

    public static void Report(EGameEventType type, string id = "", int value = 1)
    {
        OnReported?.Invoke(type, id, value);
    }
}
