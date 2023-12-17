using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Dummy : Enemy
{
    protected override IEnumerator DeadEventCo()
    {
        yield return null;
        Destroy(this.gameObject);
    }

    protected override void CallDamageEvent() {}
}
