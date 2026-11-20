using UnityEngine;

public static class CharacterPositionUtil
{
    public static Vector3 GetOverHeadPos(Renderer renderer)
    {
        var bounds = renderer.bounds;
        return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
    }
}
