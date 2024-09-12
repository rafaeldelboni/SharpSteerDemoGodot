using SharpSteer2.Obstacles;

public class EnemyVehicle(Seeker seeker) : Vehicle
{
    readonly SeekerVehicle seeker = seeker.Vehicle;

    public void Update(float elapsedTime, IEnumerable<IObstacle> obstacles)
    {
        // determine upper bound for pursuit prediction time
        var seekerToGoalDist = NVector3.Distance(Globals.HomeBaseCenter, seeker.Position);
        var adjustedDistance = seekerToGoalDist - Radius - Globals.BaseRadius;
        var seekerToGoalTime = adjustedDistance < 0 ? 0 : adjustedDistance / seeker.Speed;
        var maxPredictionTime = seekerToGoalTime * 0.9f;

        // determine steering (pursuit, obstacle avoidance, or braking)
        var steer = NVector3.Zero;
        if (seeker.State is SeekerState.Running)
        {
            var avoidance = SteerToAvoidObstacles(Globals.AvoidancePredictTimeMin, obstacles);

            // saved for annotation
            avoiding = avoidance == NVector3.Zero;

            steer = avoiding ? SteerForPursuit(seeker, maxPredictionTime) : avoidance;
        }
        else
            ApplyBrakingForce(Globals.BrakingRate, elapsedTime);

        ApplySteeringForce(steer, elapsedTime);

        // detect and record interceptions ("tags") of seeker
        var seekerToMeDist = NVector3.Distance(Position, seeker.Position);
        var sumOfRadii = Radius + seeker.Radius;

        if (seekerToMeDist >= sumOfRadii)
            return;

        switch (seeker.State)
        {
            case SeekerState.Running:
                seeker.State = SeekerState.Tagged;
                break;

            case SeekerState.Tagged:
                GD.Print("Seeker Tagged!");
                break;
        }
    }
}
