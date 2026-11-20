using NUnit.Framework;

public class DamageRuleTests
{
    private const float TOLERANCE = 0.0001f;

    // ---------- CalcDamage (규칙 36~37) ----------

    /// <summary>규칙 36: 방어 무시면 방어력이 아무리 높아도 공식을 타지 않는다.</summary>
    [Test]
    public void CalcDamage_IgnoringDefense_ReturnsRawValue()
    {
        Assert.AreEqual(100, DamageRule.CalcDamage(100f, 9999f, ignoreDFS: true));
    }

    /// <summary>규칙 37: 방어 0이면 감소가 없다. 100/(0+100) = 1.</summary>
    [Test]
    public void CalcDamage_WithZeroDefense_IsUnreduced()
    {
        Assert.AreEqual(100, DamageRule.CalcDamage(100f, 0f, ignoreDFS: false));
    }

    /// <summary>방어 100에서 정확히 절반이 된다. 공식의 기준점.</summary>
    [Test]
    public void CalcDamage_WithDefenseAtHundred_IsHalved()
    {
        Assert.AreEqual(50, DamageRule.CalcDamage(100f, 100f, ignoreDFS: false));
    }

    [TestCase(100f, 300f, 25)]
    [TestCase(200f, 100f, 100)]
    [TestCase(90f, 50f, 60)]
    public void CalcDamage_FollowsHundredOverDefensePlusHundred(float raw, float defense, int expected)
    {
        Assert.AreEqual(expected, DamageRule.CalcDamage(raw, defense, ignoreDFS: false));
    }

    /// <summary>
    /// 방어가 극단적으로 높으면 반올림되어 0이 된다. 최소 데미지 보장은 없다.
    /// 밸런싱에서 방어 상한을 두지 않으면 무피해 상태가 가능하다.
    /// </summary>
    [Test]
    public void CalcDamage_CanReachZeroWithExtremeDefense()
    {
        Assert.AreEqual(0, DamageRule.CalcDamage(100f, 100000f, ignoreDFS: false));
    }

    /// <summary>
    /// 방어 -100은 0으로 나누기가 되어 정상적인 데미지가 나오지 않는다.
    /// 현재 방어 코드가 없으므로, 밸런싱에서 방어가 -100에 닿지 않도록
    /// 막아야 한다는 사실을 테스트로 남긴다.
    /// </summary>
    [Test]
    public void CalcDamage_WithNegativeHundredDefense_IsNotMeaningful()
    {
        int result = DamageRule.CalcDamage(100f, -100f, ignoreDFS: false);

        Assert.AreNotEqual(100, result, "0 나눗셈 구간이라 원본 데미지가 보존되지 않는다");
    }

    /// <summary>방어가 음수면 데미지가 증폭된다. -50이면 두 배.</summary>
    [Test]
    public void CalcDamage_WithNegativeDefense_AmplifiesDamage()
    {
        Assert.AreEqual(200, DamageRule.CalcDamage(100f, -50f, ignoreDFS: false));
    }

    /// <summary>
    /// Mathf.RoundToInt는 .5를 짝수로 보낸다(은행가 반올림).
    /// 밸런싱 수치가 경계에 걸릴 때 의도와 어긋날 수 있어 현재 동작을 고정한다.
    /// </summary>
    [TestCase(0.5f, 0)]
    [TestCase(1.5f, 2)]
    [TestCase(2.5f, 2)]
    [TestCase(3.5f, 4)]
    public void CalcDamage_RoundsHalfToEven(float raw, int expected)
    {
        Assert.AreEqual(expected, DamageRule.CalcDamage(raw, 0f, ignoreDFS: true));
    }

    // ---------- ApplyDamage (규칙 35) ----------

    [Test]
    public void ApplyDamage_SubtractsFromCurrentHp()
    {
        Assert.AreEqual(70f, DamageRule.ApplyDamage(100f, 30), TOLERANCE);
    }

    /// <summary>규칙 35: 오버킬이어도 체력은 0에서 멈춘다.</summary>
    [Test]
    public void ApplyDamage_ClampsAtZero()
    {
        Assert.AreEqual(0f, DamageRule.ApplyDamage(50f, 9999), TOLERANCE);
    }

    [Test]
    public void ApplyDamage_ExactLethalLandsOnZero()
    {
        Assert.AreEqual(0f, DamageRule.ApplyDamage(50f, 50), TOLERANCE);
    }

    [Test]
    public void ApplyDamage_WithZeroDamage_LeavesHpUnchanged()
    {
        Assert.AreEqual(50f, DamageRule.ApplyDamage(50f, 0), TOLERANCE);
    }

    // ---------- ApplyHeal ----------

    [Test]
    public void ApplyHeal_AddsToCurrentHp()
    {
        Assert.AreEqual(80f, DamageRule.ApplyHeal(50f, 30f, 100f), TOLERANCE);
    }

    [Test]
    public void ApplyHeal_ClampsAtMaxHp()
    {
        Assert.AreEqual(100f, DamageRule.ApplyHeal(90f, 50f, 100f), TOLERANCE);
    }

    /// <summary>최대 체력도 반올림되므로 소수 최대치에서 회복 상한이 정수로 맞춰진다.</summary>
    [Test]
    public void ApplyHeal_RoundsMaxHpBeforeClamping()
    {
        Assert.AreEqual(100f, DamageRule.ApplyHeal(50f, 999f, 100.4f), TOLERANCE);
    }

    /// <summary>
    /// 이미 최대치를 넘은 상태(스탯 변동 등)에서 회복하면 최대치로 끌어내려진다.
    /// </summary>
    [Test]
    public void ApplyHeal_PullsOverfilledHpDownToMax()
    {
        Assert.AreEqual(100f, DamageRule.ApplyHeal(120f, 10f, 100f), TOLERANCE);
    }
}
