/// <summary>
/// 난수 공급원. 전역 UnityEngine.Random을 직접 부르는 대신 주입해
/// 테스트에서 결과를 고정하고, 시드를 통해 상황을 재현할 수 있게 한다.
/// </summary>
public interface IRandomSource
{
    /// <summary>minInclusive 이상 maxExclusive 미만의 정수.</summary>
    public int Range(int minInclusive, int maxExclusive);

    /// <summary>0 이상 1 이하의 실수. 1.0을 포함한다.</summary>
    public float Value();
}
