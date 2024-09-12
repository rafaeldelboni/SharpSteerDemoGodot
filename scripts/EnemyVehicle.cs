using System.Linq;
using Vector3 = System.Numerics.Vector3;

public class EnemyVehicle(
    Seeker seeker,
    ObstacleSpawner obstacleSpawner
) : Vehicle(obstacleSpawner)
{
    readonly SeekerVehicle seeker = seeker.vehicle;

    public void Update(float elapsedTime)
    {
        // determine upper bound for pursuit prediction time
        var seekerToGoalDist = Vector3.Distance(Globals.HomeBaseCenter, seeker.Position);
        var adjustedDistance = seekerToGoalDist - Radius - Globals.BaseRadius;
        var seekerToGoalTime = adjustedDistance < 0 ? 0 : adjustedDistance / seeker.Speed;
        var maxPredictionTime = seekerToGoalTime * 0.9f;

        // determine steering (pursuit, obstacle avoidance, or braking)
        var steer = Vector3.Zero;
        if (seeker.State == SeekerState.Running)
        {
            var allObstacles = obstacleSpawner.allObstacles.Select(o => o.sphericalObstacle);
            var avoidance = SteerToAvoidObstacles(Globals.AvoidancePredictTimeMin, allObstacles);

            // saved for annotation
            avoiding = avoidance == Vector3.Zero;

            steer = avoiding ? SteerForPursuit(seeker, maxPredictionTime) : avoidance;
        }
        else
            ApplyBrakingForce(Globals.BrakingRate, elapsedTime);

        ApplySteeringForce(steer, elapsedTime);

        // detect and record interceptions ("tags") of seeker
        var seekerToMeDist = Vector3.Distance(Position, seeker.Position);
        var sumOfRadii = Radius + seeker.Radius;

        if (seekerToMeDist >= sumOfRadii)
            return;

        switch (seeker.State)
        {
            case SeekerState.Running:
                seeker.State = SeekerState.Tagged;
                break;

            case SeekerState.Tagged:
                Godot.GD.Print("Seeker Tagged!");
                break;
        }
    }
}
