using NUnit.Framework;

public class ResistRuleTests
{
    /// <summary>규칙 22: 저항률 0이면 어떤 굴림이 나와도 효과가 들어간다.</summary>
    [TestCase(0f)]
    [TestCase(0.5f)]
    [TestCase(0.999f)]
    public void IsResisted_AtZeroRate_NeverResists(float roll)
    {
        var random = new FixedRandomSource(fixedValue: roll);

        Assert.IsFalse(ResistRule.IsResisted(EffectType.Stun, 0f, random));
    }

    [Test]
    public void IsResisted_IsTrueWhenRollLandsBelowRate()
    {
        var random = new FixedRandomSource(fixedValue: 0.29f);

        Assert.IsTrue(ResistRule.IsResisted(EffectType.Stun, 0.3f, random));
    }

    /// <summary>굴림이 저항률과 같으면 저항 실패다. 경계는 열린 구간이다.</summary>
    [Test]
    public void IsResisted_IsFalseWhenRollEqualsRate()
    {
        var random = new FixedRandomSource(fixedValue: 0.3f);

        Assert.IsFalse(ResistRule.IsResisted(EffectType.Stun, 0.3f, random));
    }

    /// <summary>
    /// 규칙 23: 저항률 1.0은 완전 면역이다. Random.value가 1.0을 포함하기 때문에
    /// 굴림에 맡기면 1.0이 나왔을 때 저항이 뚫린다.
    /// </summary>
    [Test]
    public void IsResisted_AtFullRate_IsImmuneEvenWhenRollIsMaximum()
    {
        var random = new FixedRandomSource(fixedValue: 1f);

        Assert.IsTrue(ResistRule.IsResisted(EffectType.Stun, 1f, random));
    }

    [Test]
    public void IsResisted_AboveFullRate_StaysImmune()
    {
        var random = new FixedRandomSource(fixedValue: 1f);

        Assert.IsTrue(ResistRule.IsResisted(EffectType.Stun, 2.5f, random));
    }

    /// <summary>완전 면역 구간에서는 굴림을 소모하지 않는다.</summary>
    [Test]
    public void IsResisted_AtFullRate_DoesNotConsumeRoll()
    {
        var random = new FixedRandomSource(fixedValue: 0f);

        ResistRule.IsResisted(EffectType.Stun, 1f, random);

        Assert.AreEqual(0, random.ValueCallCount);
    }

    /// <summary>규칙 24: 저항 맵에 없는 타입은 굴림 없이 항상 통과한다.</summary>
    [TestCase(EffectType.Damage)]
    [TestCase(EffectType.Buff_ATKFlat)]
    [TestCase(EffectType.Hp_Recovery_Flat)]
    public void IsResisted_IsFalseForNonResistableTypes(EffectType type)
    {
        var random = new FixedRandomSource(fixedValue: 0f);

        Assert.IsFalse(ResistRule.IsResisted(type, 1f, random),
            "저항 대상이 아닌 타입은 저항률이 아무리 높아도 통과한다");
    }

    [Test]
    public void IsResisted_ForNonResistableType_DoesNotConsumeRoll()
    {
        var random = new FixedRandomSource(fixedValue: 0f);

        ResistRule.IsResisted(EffectType.Damage, 0.5f, random);

        Assert.AreEqual(0, random.ValueCallCount);
    }

    [TestCase(EffectType.Stun)]
    [TestCase(EffectType.Bleed)]
    [TestCase(EffectType.Blight)]
    public void IsResisted_RollsForEveryResistableType(EffectType type)
    {
        var random = new FixedRandomSource(fixedValue: 0f);

        ResistRule.IsResisted(type, 0.5f, random);

        Assert.AreEqual(1, random.ValueCallCount);
    }

    /// <summary>같은 굴림 순서를 주면 결과도 같아야 한다. 시드 재현의 전제.</summary>
    [Test]
    public void IsResisted_IsDeterministicForAGivenRollSequence()
    {
        var random = new FixedRandomSource().EnqueueValues(0.1f, 0.9f, 0.2f);

        Assert.IsTrue(ResistRule.IsResisted(EffectType.Stun, 0.5f, random));
        Assert.IsFalse(ResistRule.IsResisted(EffectType.Stun, 0.5f, random));
        Assert.IsTrue(ResistRule.IsResisted(EffectType.Stun, 0.5f, random));
    }
}
