using Vector3 = System.Numerics.Vector3;

static class Globals
{
    public static float BaseRadius = .5f;
    public static readonly Vector3 HomeBaseCenter = new(0, 0, 0);

    public const float MinStartRadius = 30;
    public const float MaxStartRadius = 40;

    public const float BrakingRate = 0.75f;

    public const float AvoidancePredictTimeMin = 0.9f;
    public const float AvoidancePredictTimeMax = 2;
    public static float AvoidancePredictTime = AvoidancePredictTimeMin;

    // count the number of times the simulation has reset (e.g. for overnight runs)
    public static int ResetCount = 0;
}
