using UnityEngine;

public enum ECursorType
{
    Nomal,
    Upgrade,
}

public class CursorManager : Singleton<CursorManager>
{
    public bool IsDrag { get; set; }
}
