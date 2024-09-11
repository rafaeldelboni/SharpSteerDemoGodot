using System.Linq;
using SharpSteer2.Helpers;
using Vector3 = System.Numerics.Vector3;

public enum SeekerState
{
    Running,
    Tagged,
    AtGoal
}

public class SeekerVehicle : Vehicle
{
    readonly bool arrive;
    public SeekerState State;
    bool evading; // xxx store steer sub-state for anotation
    float lastRunningTime; // for auto-reset

    EnemySpawner enemySpawner;

    public SeekerVehicle(EnemySpawner enemySpawner, ObstacleSpawner obstacleSpawner) : base(obstacleSpawner)
    {
        this.enemySpawner = enemySpawner;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        State = SeekerState.Running;
        evading = false;
    }

    // per frame simulation update
    public void Update(float currentTime, float elapsedTime)
    {
        // do behavioral state transitions, as needed
        UpdateState(currentTime);

        // determine and apply steering/braking forces
        var steer = Vector3.Zero;
        if (State == SeekerState.Running)
        {
            steer = SteeringForSeeker();
        }
        else
        {
            ApplyBrakingForce(Globals.BrakingRate, elapsedTime);
        }

        ApplySteeringForce(steer, elapsedTime);
    }

    // is there a clear path to the goal?
    bool IsPathToGoalClear()
    {
        var sideThreshold = Radius * 8.0f;
        var behindThreshold = Radius * 2.0f;

        var goalOffset = Globals.HomeBaseCenter - Position;
        var goalDistance = goalOffset.Length();
        var goalDirection = goalOffset / goalDistance;

        var goalIsAside = this.IsAside(Globals.HomeBaseCenter, 0.5f);

        // for annotation: loop over all and save result, instead of early return
        var xxxReturn = true;

        // loop over enemies
        foreach (var e in enemySpawner.allEnemies.Select(e => e.vehicle))
        {
            var eDistance = Vector3.Distance(Position, e.Position);
            var timeEstimate = 0.3f * eDistance / e.Speed; //xxx
            var eFuture = e.PredictFuturePosition(timeEstimate);
            var eOffset = eFuture - Position;
            var alongCorridor = Vector3.Dot(goalDirection, eOffset);
            var inCorridor = alongCorridor > -behindThreshold && alongCorridor < goalDistance;
            var eForwardDistance = Vector3.Dot(Forward, eOffset);

            // consider as potential blocker if within the corridor
            if (inCorridor)
            {
                var perp = eOffset - (goalDirection * alongCorridor);
                var acrossCorridor = perp.Length();
                if (acrossCorridor < sideThreshold)
                {
                    // not a blocker if behind us and we are perp to corridor
                    var eFront = eForwardDistance + e.Radius;

                    var eIsBehind = eFront < -behindThreshold;
                    var eIsWayBehind = eFront < -2 * behindThreshold;
                    var safeToTurnTowardsGoal = (eIsBehind && goalIsAside) || eIsWayBehind;

                    if (!safeToTurnTowardsGoal)
                    {
                        xxxReturn = false;
                    }
                }
            }
        }

        ClearPathAnnotation(sideThreshold, behindThreshold, goalDirection);
        return xxxReturn;
    }

    Vector3 SteeringForSeeker()
    {
        // determine if obstacle avodiance is needed
        var clearPath = IsPathToGoalClear();
        AdjustObstacleAvoidanceLookAhead(clearPath);
        var AllObstacles = obstacleSpawner.allObstacles.Select(o => o.sphericalObstacle);
        var obstacleAvoidance = SteerToAvoidObstacles(Globals.AvoidancePredictTime, AllObstacles);

        // saved for annotation
        avoiding = obstacleAvoidance != Vector3.Zero;

        if (avoiding)
        {
            // use pure obstacle avoidance if needed
            return obstacleAvoidance;
        }

        // otherwise seek home base and perhaps evade defenders
        var seek = !arrive
            ? SteerForSeek(Globals.HomeBaseCenter)
            : this.SteerForArrival(Globals.HomeBaseCenter, MaxSpeed, 10, Annotation);

        if (clearPath)
        {
            // we have a clear path (defender-free corridor), use pure seek
            Godot.GD.Print("Seeker has clear path.");
            return seek;
        }

        var evade = XxxSteerToEvadeAllDefenders();
        var steer = (seek + evade).LimitMaxDeviationAngle(0.707f, Forward);

        return steer;
    }

    void UpdateState(float currentTime)
    {
        // if we reach the goal before being tagged, switch to atGoal state
        if (State == SeekerState.Running)
        {
            var baseDistance = Vector3.Distance(Position, Globals.HomeBaseCenter);
            if (baseDistance < Radius + Globals.BaseRadius)
            {
                Godot.GD.Print("Seeker At Goal!");
                State = SeekerState.AtGoal;
            }
        }

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
                Godot.GD.Print("Out of time.");
            }
        }
    }
    public Vector3 SteerToEvadeAllDefenders()
    {
        var evade = Vector3.Zero;
        var goalDistance = Vector3.Distance(Globals.HomeBaseCenter, Position);

        // sum up weighted evasion
        foreach (var e in enemySpawner.allEnemies.Select(e => e.vehicle))
        {
            var eOffset = e.Position - Position;
            var eDistance = eOffset.Length();

            var eForwardDistance = Vector3.Dot(Forward, eOffset);
            var behindThreshold = Radius * 2;
            var behind = eForwardDistance < behindThreshold;
            if (!behind || eDistance < 5)
            {
                if (eDistance < goalDistance * 1.2) //xxx
                {
                    var timeEstimate = 0.15f * eDistance / e.Speed; //xxx
                    var future = e.PredictFuturePosition(timeEstimate);

                    var offset = future - Position;
                    var lateral = Vector3Helpers.PerpendicularComponent(offset, Forward);
                    var d = lateral.Length();
                    var weight = -1000 / (d * d);
                    evade += lateral / d * weight;
                }
            }
        }

        return evade;
    }

    Vector3 XxxSteerToEvadeAllDefenders()
    {
        // sum up weighted evasion
        var evade = Vector3.Zero;
        foreach (var e in enemySpawner.allEnemies.Select(e => e.vehicle))
        {
            var eOffset = e.Position - Position;
            var eDistance = eOffset.Length();

            // xxx maybe this should take into account e's heading? xxx
            var timeEstimate = 0.5f * eDistance / e.Speed; //xxx
            var eFuture = e.PredictFuturePosition(timeEstimate);

            // steering to flee from eFuture (enemy's future position)
            var flee = SteerForFlee(eFuture);

            var eForwardDistance = Vector3.Dot(Forward, eOffset);
            var behindThreshold = Radius * -2;

            var distanceWeight = 4 / eDistance;
            var forwardWeight = eForwardDistance > behindThreshold ? 1.0f : 0.5f;

            var adjustedFlee = flee * distanceWeight * forwardWeight;

            evade += adjustedFlee;
        }

        return evade;
    }

    void AdjustObstacleAvoidanceLookAhead(bool clearPath)
    {
        if (clearPath)
        {
            evading = false;
            var goalDistance = Vector3.Distance(Globals.HomeBaseCenter, Position);
            var headingTowardGoal = this.IsAhead(Globals.HomeBaseCenter, 0.98f);
            var isNear = goalDistance / Speed < Globals.AvoidancePredictTimeMax;
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
        var behindBack = Forward * -behindThreshold;
        var pbb = Position + behindBack;
        var gun = this.LocalRotateForwardToSide(goalDirection);
        var gn = gun * sideThreshold;
        var hbc = Globals.HomeBaseCenter;
    }
}
