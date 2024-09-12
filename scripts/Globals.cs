global using Godot;
global using System.Linq;
global using System.Collections.Generic;
global using NVector3 = System.Numerics.Vector3;

static class Globals
{
    public const float BaseRadius = .5f;
    public static readonly NVector3 HomeBaseCenter = NVector3.Zero;

    public const float MinStartRadius = 30;
    public const float MaxStartRadius = 40;

    public const float BrakingRate = 0.75f;

    public const float AvoidancePredictTimeMin = 0.9f;
    public const float AvoidancePredictTimeMax = 2;
    public static float AvoidancePredictTime { get; private set; } = AvoidancePredictTimeMin;

    public static void UseMinPredictTime() => AvoidancePredictTime = AvoidancePredictTimeMin;
    public static void UseMaxPredictTime() => AvoidancePredictTime = AvoidancePredictTimeMax;

    // count the number of times the simulation has reset (e.g. for overnight runs)
    // public static int ResetCount = 0;
}
