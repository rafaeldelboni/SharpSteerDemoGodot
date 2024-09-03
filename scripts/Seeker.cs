using Godot;
using SharpSteer2;
using SharpSteer2.Helpers;
using SharpSteer2.Obstacles;
using System.Linq;
using System.Collections.Generic;

public enum SeekerState
{
    Running,
    Tagged,
    AtGoal
}

public class Vehicle : SimpleVehicle
{
    public override float MaxForce => 3;
    public override float MaxSpeed => 3;

    public override void Reset()
    {
        base.Reset();  // reset the vehicle
        Speed = 3;     // speed along Forward direction.
    }
}

public partial class Seeker : Node3D
{
    public SeekerState State;
    Vehicle vehicle;
    bool evading; // xxx store steer sub-state for anotation
    protected bool avoiding;
    readonly bool arrive;
    double lastRunningTime; // for auto-reset

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        vehicle = new();
        Reset();
        vehicle.Position = Position.ToNumerics();
        InitializeObstacles(0.5f, 100);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // do behavioral state transitions, as needed
        UpdateState(delta);

        // determine and apply steering/braking forces
        var steer = Vector3.Zero;
        if (State == SeekerState.Running)
        {
            steer = SteeringForSeeker();
        }
        else
        {
            vehicle.ApplyBrakingForce(Globals.BrakingRate, (float)delta);
        }

        vehicle.ApplySteeringForce(steer.ToNumerics(), ((float)delta));
        Position = vehicle.Position.ToGodot();

        GD.Print("State ", State, Position);
    }

    public void Reset()
    {
        vehicle.Reset();
        State = SeekerState.Running;
        evading = false;
    }

    void UpdateState(double currentTime)
    {
        // if we reach the goal before being tagged, switch to atGoal state
        if (State == SeekerState.Running)
        {
            var baseDistance = Position.DistanceTo(Globals.HomeBaseCenter);
            if (baseDistance < vehicle.Radius + baseRadius) State = SeekerState.AtGoal;
        }

        // update lastRunningTime (holds off reset time)
        if (State == SeekerState.Running)
        {
            lastRunningTime = currentTime;
        }
        else
        {
            const float resetDelay = 4;
            var resetTime = lastRunningTime + resetDelay;
            if (currentTime > resetTime)
            {
                Reset();
            }
        }
    }

    Vector3 SteeringForSeeker()
    {
        // determine if obstacle avodiance is needed
        var clearPath = IsPathToGoalClear();
        AdjustObstacleAvoidanceLookAhead(clearPath);
        var obstacleAvoidance = vehicle.SteerToAvoidObstacles(Globals.AvoidancePredictTime, AllObstacles).ToGodot();

        // saved for annotation
        avoiding = obstacleAvoidance != Vector3.Zero;

        if (avoiding)
        {
            // use pure obstacle avoidance if needed
            return obstacleAvoidance;
        }

        // otherwise seek home base and perhaps evade defenders
        var seek = !arrive
            ? vehicle.SteerForSeek(Globals.HomeBaseCenter.ToNumerics(), vehicle.MaxSpeed, null)
            : vehicle.SteerForArrival(Globals.HomeBaseCenter.ToNumerics(), vehicle.MaxSpeed, 10, null);


        if (clearPath)
        {
            // we have a clear path (defender-free corridor), use pure seek
            return seek.ToGodot();
        }

        var evade = XxxSteerToEvadeAllDefenders();
        var steer = (seek + evade.ToNumerics()).LimitMaxDeviationAngle(0.707f, Vector3.Forward.ToNumerics());

        // annotation: show evasion steering force
        return steer.ToGodot();
    }

    Vector3 XxxSteerToEvadeAllDefenders()
    {
        // sum up weighted evasion
        var evade = Vector3.Zero;
        // foreach (var e in plugin.CtfEnemies)
        // {
        //     var eOffset = e.Position - Position;
        //     var eDistance = eOffset.Length();
        //
        //     // xxx maybe this should take into account e's heading? xxx
        //     var timeEstimate = 0.5f * eDistance / e.Speed; //xxx
        //     var eFuture = e.PredictFuturePosition(timeEstimate);
        //
        //
        //     // steering to flee from eFuture (enemy's future position)
        //     var flee = vehicle.SteerForFlee(eFuture);
        //
        //     var eForwardDistance = Vector3.Dot(Vector3.Forward, eOffset);
        //     var behindThreshold = vehicle.Radius * -2;
        //
        //     var distanceWeight = 4 / eDistance;
        //     var forwardWeight = eForwardDistance > behindThreshold ? 1.0f : 0.5f;
        //
        //     var adjustedFlee = flee * distanceWeight * forwardWeight;
        //
        //     evade += adjustedFlee;
        // }

        return evade;
    }

    void AdjustObstacleAvoidanceLookAhead(bool clearPath)
    {
        if (clearPath)
        {
            evading = false;
            var goalDistance = Globals.HomeBaseCenter.DistanceTo(Position);
            var headingTowardGoal = vehicle.IsAhead(Globals.HomeBaseCenter.ToNumerics(), 0.98f);
            var isNear = goalDistance / vehicle.Speed < Globals.AvoidancePredictTimeMax;
            var useMax = headingTowardGoal && !isNear;
            Globals.AvoidancePredictTime = useMax ? Globals.AvoidancePredictTimeMax : Globals.AvoidancePredictTimeMin;
        }
        else
        {
            evading = true;
            Globals.AvoidancePredictTime = Globals.AvoidancePredictTimeMin;
        }
    }


    void ClearPathAnnotation(float sideThreshold, float behindThreshold, Vector3 goalDirection)
    {
        var behindBack = Vector3.Forward * -behindThreshold;
        var pbb = Position + behindBack;
        var gun = vehicle.LocalRotateForwardToSide(goalDirection.ToNumerics());
        var gn = gun * sideThreshold;
        var hbc = Globals.HomeBaseCenter;
    }

    // is there a clear path to the goal?
    bool IsPathToGoalClear()
    {
        var sideThreshold = vehicle.Radius * 8.0f;
        var behindThreshold = vehicle.Radius * 2.0f;

        var goalOffset = Globals.HomeBaseCenter - Position;
        var goalDistance = goalOffset.Length();
        var goalDirection = goalOffset / goalDistance;

        var goalIsAside = vehicle.IsAside(Globals.HomeBaseCenter.ToNumerics(), 0.5f);

        // for annotation: loop over all and save result, instead of early return
        var xxxReturn = true;

        // loop over enemies
        // foreach (var e in plugin.CtfEnemies)
        // {
        //     var eDistance = Vector3.DistanceTo(Position, e.Position);
        //     var timeEstimate = 0.3f * eDistance / e.Speed; //xxx
        //     var eFuture = e.PredictFuturePosition(timeEstimate);
        //     var eOffset = eFuture - Position;
        //     var alongCorridor = Vector3.Dot(goalDirection, eOffset);
        //     var inCorridor = alongCorridor > -behindThreshold && alongCorridor < goalDistance;
        //     var eForwardDistance = Vector3.Dot(Vector3.Forward, eOffset);
        //
        //     // consider as potential blocker if within the corridor
        //     if (inCorridor)
        //     {
        //         var perp = eOffset - (goalDirection * alongCorridor);
        //         var acrossCorridor = perp.Length();
        //         if (acrossCorridor < sideThreshold)
        //         {
        //             // not a blocker if behind us and we are perp to corridor
        //             var eFront = eForwardDistance + e.Radius;
        //
        //             //annotation.annotationLine (position, forward*eFront, gGreen); // xxx
        //             //annotation.annotationLine (e.position, forward*eFront, gGreen); // xxx
        //
        //             // xxx
        //             // std::ostringstream message;
        //             // message << "eFront = " << std::setprecision(2)
        //             //         << std::setiosflags(std::ios::fixed) << eFront << std::ends;
        //             // draw2dTextAt3dLocation (*message.str(), eFuture, gWhite);
        //
        //             var eIsBehind = eFront < -behindThreshold;
        //             var eIsWayBehind = eFront < -2 * behindThreshold;
        //             var safeToTurnTowardsGoal = (eIsBehind && goalIsAside) || eIsWayBehind;
        //
        //             if (!safeToTurnTowardsGoal)
        //             {
        //                 // return false;
        //                 xxxReturn = false;
        //             }
        //         }
        //     }
        // }

        // no enemies found along path, return true to indicate path is clear
        // clearPathAnnotation (sideThreshold, behindThreshold, goalDirection);
        // return true;
        //if (xxxReturn)
        ClearPathAnnotation(sideThreshold, behindThreshold, goalDirection);
        return xxxReturn;
    }

    // move to scene begin
    float baseRadius = 1.5f;
    public static readonly List<SphericalObstacle> AllObstacles = new();
    protected static int obstacleCount = -1;

    // dynamic obstacle registry
    public static void InitializeObstacles(float radius, int obstacles)
    {
        // start with 40% of possible obstacles
        if (obstacleCount == -1)
        {
            obstacleCount = 0;
            for (var i = 0; i < obstacles; i++)
                AddOneObstacle(radius);
        }
    }

    public static void AddOneObstacle(float radius)
    {
        // pick a random center and radius,
        // loop until no overlap with other obstacles and the home base
        float r;
        Vector3 c;
        float minClearance;
        // todo use player radius
        var requiredClearance = 0.5f * 4; // 2 x diameter
        do
        {
            r = RandomHelpers.Random(1.5f, 4);
            c = Vector3Helpers.RandomVectorOnUnitRadiusXZDisk().ToGodot() * Globals.MaxStartRadius * 1.1f;
            minClearance = AllObstacles.Aggregate(float.MaxValue, (current, t) => TestOneObstacleOverlap(current, r, t.Radius, c, t.Center.ToGodot()));

            minClearance = TestOneObstacleOverlap(minClearance, r, radius - requiredClearance, c, Globals.HomeBaseCenter);
        }
        while (minClearance < requiredClearance);

        // add new non-overlapping obstacle to registry
        AllObstacles.Add(new(r, c.ToNumerics()));
        obstacleCount++;
    }

    public static void RemoveOneObstacle()
    {
        if (obstacleCount <= 0)
            return;

        obstacleCount--;
        AllObstacles.RemoveAt(obstacleCount);
    }

    static float MinDistanceToObstacle(Vector3 point)
    {
        const float r = 0;
        var c = point;
        return AllObstacles.Aggregate(float.MaxValue, (current, t) => TestOneObstacleOverlap(current, r, t.Radius, c, t.Center.ToGodot()));
    }

    static float TestOneObstacleOverlap(float minClearance, float r, float radius, Vector3 c, Vector3 center)
    {
        var d = c.DistanceTo(center);
        var clearance = d - (r + radius);
        if (minClearance > clearance) minClearance = clearance;
        return minClearance;
    }
    // move to scene end

}
