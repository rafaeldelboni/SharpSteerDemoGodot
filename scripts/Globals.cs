using Godot;

static class Globals
{
    public static readonly Vector3 HomeBaseCenter = new(0, 0, 0);

    public const float MinStartRadius = 30;
    public const float MaxStartRadius = 40;

    public const float BrakingRate = 0.75f;

    public static readonly Color
        EvadeColor = new((byte)(255.0f * 0.6f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.3f)); // annotation

    public static readonly Color
        SeekColor = new((byte)(255.0f * 0.3f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.6f)); // annotation

    public static readonly Color ClearPathColor =
        new((byte)(255.0f * 0.3f), (byte)(255.0f * 0.6f), (byte)(255.0f * 0.3f)); // annotation

    public const float AvoidancePredictTimeMin = 0.9f;
    public const float AvoidancePredictTimeMax = 2;
    public static float AvoidancePredictTime = AvoidancePredictTimeMin;

    // count the number of times the simulation has reset (e.g. for overnight runs)
    public static int ResetCount = 0;
}
