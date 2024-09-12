using SharpSteer2.Obstacles;

public class EnemyVehicle(SeekerVehicle seeker) : Vehicle
{
    public SeekerVehicle Seeker { get; set; } = seeker;

    public void Update(float elapsedTime, IEnumerable<IObstacle> obstacles)
    {
        // determine upper bound for pursuit prediction time
        var seekerToGoalDist = NVector3.Distance(Globals.HomeBaseCenter, Seeker.Position);
        var adjustedDistance = seekerToGoalDist - Radius - Globals.BaseRadius;
        var seekerToGoalTime = adjustedDistance < 0 ? 0 : adjustedDistance / Seeker.Speed;
        var maxPredictionTime = seekerToGoalTime * 0.9f;

        // determine steering (pursuit, obstacle avoidance, or braking)
        var steer = NVector3.Zero;
        if (Seeker.State is SeekerState.Running)
        {
            var avoidance = SteerToAvoidObstacles(Globals.AvoidancePredictTimeMin, obstacles);

            // saved for annotation
            avoiding = avoidance == NVector3.Zero;

            steer = avoiding ? SteerForPursuit(Seeker, maxPredictionTime) : avoidance;
        }
        else
            ApplyBrakingForce(Globals.BrakingRate, elapsedTime);

        ApplySteeringForce(steer, elapsedTime);

        // detect and record interceptions ("tags") of seeker
        var seekerToMeDist = NVector3.Distance(Position, Seeker.Position);
        var sumOfRadii = Radius + Seeker.Radius;

        if (seekerToMeDist >= sumOfRadii)
            return;

        switch (Seeker.State)
        {
            case SeekerState.Running:
                Seeker.State = SeekerState.Tagged;
                break;

            case SeekerState.Tagged:
                GD.Print("Seeker Tagged!");
                break;
        }
    }
}
