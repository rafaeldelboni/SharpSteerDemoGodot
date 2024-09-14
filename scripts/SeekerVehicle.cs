using SharpSteer2.Helpers;
using SharpSteer2.Obstacles;

public enum SeekerState
{
    Running,
    Tagged,
    AtGoal,
}

public class SeekerVehicle : Vehicle
{
    const float ResetDelay = 4;

    float lastRunningTime; // for auto-reset
    public SeekerState State = SeekerState.Running;
    bool arrive = false; // TODO: not being used 🤔

    public override void Reset()
    {
        base.Reset();
        State = SeekerState.Running;
    }

    // per frame simulation update
    public void Update(
        float currentTime,
        float elapsedTime,
        IEnumerable<IObstacle> obstacles,
        Enemy[] enemies
    )
    {
        // do behavioral state transitions, as needed
        UpdateState(currentTime);

        // determine and apply steering/braking forces
        var steer = NVector3.Zero;
        if (State is SeekerState.Running)
            steer = SteeringForSeeker(enemies, obstacles);
        else
            ApplyBrakingForce(Globals.BrakingRate, elapsedTime);

        ApplySteeringForce(steer, elapsedTime);
    }

    // is there a clear path to the goal?
    bool IsPathToGoalClear(Enemy[] enemies)
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
        foreach (var enemy in enemies)
        {
            var vehicle = enemy.Vehicle;
            var eDistance = NVector3.Distance(Position, vehicle.Position);
            var timeEstimate = 0.3f * eDistance / vehicle.Speed; //xxx
            var eFuture = vehicle.PredictFuturePosition(timeEstimate);
            var eOffset = eFuture - Position;
            var alongCorridor = NVector3.Dot(goalDirection, eOffset);
            var inCorridor = alongCorridor > -behindThreshold && alongCorridor < goalDistance;
            var eForwardDistance = NVector3.Dot(Forward, eOffset);

            // consider as potential blocker if within the corridor
            if (inCorridor)
            {
                var perp = eOffset - (goalDirection * alongCorridor);
                var acrossCorridor = perp.Length();
                if (acrossCorridor < sideThreshold)
                {
                    // not a blocker if behind us and we are perp to corridor
                    var eFront = eForwardDistance + vehicle.Radius;

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

    NVector3 SteeringForSeeker(Enemy[] enemies, IEnumerable<IObstacle> obstacles)
    {
        // determine if obstacle avoidance is needed
        var clearPath = IsPathToGoalClear(enemies);
        AdjustObstacleAvoidanceLookAhead(clearPath);
        var obstacleAvoidance = SteerToAvoidObstacles(
            Globals.AvoidancePredictTime,
            obstacles
        );

        // saved for annotation
        avoiding = obstacleAvoidance != NVector3.Zero;

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
            GD.Print("Seeker has clear path.");
            return seek;
        }

        var evade = XxxSteerToEvadeAllDefenders(enemies);
        var steer = (seek + evade).LimitMaxDeviationAngle(0.707f, Forward);

        return steer;
    }

    void UpdateState(float currentTime)
    {
        // if we reach the goal before being tagged, switch to atGoal state
        if (State == SeekerState.Running)
        {
            var baseDistance = NVector3.Distance(Position, Globals.HomeBaseCenter);
            if (baseDistance < Radius + Globals.BaseRadius)
            {
                GD.Print("Seeker At Goal!");
                State = SeekerState.AtGoal;
            }
        }

        if (State is SeekerState.Running)
        {
            lastRunningTime = currentTime;
        }
        else
        {
            var resetTime = lastRunningTime + ResetDelay;
            if (currentTime > resetTime)
                GD.Print("Out of time.");
        }
    }

    // TODO: not being called
    public NVector3 SteerToEvadeAllDefenders(Enemy[] enemies)
    {
        var evade = NVector3.Zero;
        var goalDistance = NVector3.Distance(Globals.HomeBaseCenter, Position);

        // sum up weighted evasion
        foreach (var enemy in enemies)
        {
            var vehicle = enemy.Vehicle;
            var eOffset = vehicle.Position - Position;
            var eDistance = eOffset.Length();

            var eForwardDistance = NVector3.Dot(Forward, eOffset);
            var behindThreshold = Radius * 2;
            var behind = eForwardDistance < behindThreshold;

            if (behind && eDistance >= 5) continue;
            if (eDistance >= goalDistance * 1.2) continue;

            var timeEstimate = 0.15f * eDistance / vehicle.Speed;
            var future = vehicle.PredictFuturePosition(timeEstimate);

            var offset = future - Position;
            var lateral = Vector3Helpers.PerpendicularComponent(offset, Forward);
            var d = lateral.Length();
            var weight = -1000 / (d * d);
            evade += lateral / d * weight;
        }

        return evade;
    }

    NVector3 XxxSteerToEvadeAllDefenders(Enemy[] enemies)
    {
        // sum up weighted evasion
        var evade = NVector3.Zero;
        foreach (var enemy in enemies)
        {
            var vehicle = enemy.Vehicle;
            var eOffset = vehicle.Position - Position;
            var eDistance = eOffset.Length();

            // xxx maybe this should take into account e's heading? xxx
            var timeEstimate = 0.5f * eDistance / vehicle.Speed; //xxx
            var eFuture = vehicle.PredictFuturePosition(timeEstimate);

            // steering to flee from eFuture (enemy's future position)
            var flee = SteerForFlee(eFuture);

            var eForwardDistance = NVector3.Dot(Forward, eOffset);
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
            var goalDistance = NVector3.Distance(Globals.HomeBaseCenter, Position);
            var headingTowardGoal = this.IsAhead(Globals.HomeBaseCenter, 0.98f);
            var isNear = goalDistance / Speed < Globals.AvoidancePredictTimeMax;
            var useMax = headingTowardGoal && !isNear;

            if (useMax)
                Globals.UseMaxPredictTime();
            else
                Globals.UseMinPredictTime();
        }
        else
            Globals.UseMinPredictTime();
    }

    // TODO: not doing anything
    void ClearPathAnnotation(float sideThreshold, float behindThreshold, NVector3 goalDirection)
    {
        // var behindBack = Forward * -behindThreshold;
        // var pbb = Position + behindBack;
        // var gun = this.LocalRotateForwardToSide(goalDirection);
        // var gn = gun * sideThreshold;
        // var hbc = Globals.HomeBaseCenter;
    }
}
