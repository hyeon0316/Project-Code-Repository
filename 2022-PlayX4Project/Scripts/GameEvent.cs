using System;

public static class GameEvent 
{
    public static event Action EnableEndingChestEvent;
    public static event Action<MapType> MapChangeEvent;

    public static void CallEndingChest()
    {
        EnableEndingChestEvent?.Invoke();
    }

    public static void CallMapChange(MapType type)
    {
        MapChangeEvent?.Invoke(type);
    }
}
