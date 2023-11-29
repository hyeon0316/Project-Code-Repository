using System;

public static class GameEvent 
{
    public static Action EnableEndingChestEvent;

    public static void CallEndingChest()
    {
        EnableEndingChestEvent?.Invoke();
    }

    public static void ClearEvents()
    {
        EnableEndingChestEvent = null;
    }
}
