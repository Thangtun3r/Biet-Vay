using System;

public static class WordPoolEvents
{
    /// ✅ Broadcast when you want to clear all word pools
    public static event Action OnClearPools;

    /// ✅ Broadcast when you want to toggle word pool ON/OFF
    public static event Action<bool> OnTogglePools;

    public static void BroadcastClear()
    {
        OnClearPools?.Invoke();
    }

    public static void BroadcastToggle(bool isEnabled)
    {
        OnTogglePools?.Invoke(isEnabled);
    }
}