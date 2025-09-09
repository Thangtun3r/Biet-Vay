public interface ISpeedModifier
{
    bool Tick(float dt); // return false when expired
}

// MULTIPLIER: e.g. x1.5 (+50%)
public class TimedMultiplierSpeedMod : ISpeedModifier
{
    private float timeLeft;
    public readonly float Multiplier;

    public TimedMultiplierSpeedMod(float duration, float multiplier)
    { timeLeft = duration; Multiplier = multiplier; }

    public bool Tick(float dt) { timeLeft -= dt; return timeLeft > 0f; }
}

// ADDITIVE: e.g. +2.0 speed (flat)
public class TimedAdditiveSpeedMod : ISpeedModifier
{
    private float timeLeft;
    public readonly float Additive;

    public TimedAdditiveSpeedMod(float duration, float additive)
    { timeLeft = duration; Additive = additive; }

    public bool Tick(float dt) { timeLeft -= dt; return timeLeft > 0f; }
}