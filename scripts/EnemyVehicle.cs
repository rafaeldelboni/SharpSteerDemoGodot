using System.Linq;
using Vector3 = System.Numerics.Vector3;

public class EnemyVehicle : Vehicle
{
    SeekerVehicle seeker;

    public EnemyVehicle(Seeker seeker, ObstacleSpawner obstacleSpawner) : base(obstacleSpawner)
    {
        this.seeker = seeker.vehicle;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
    }

    public void Update(float currentTime, float elapsedTime)
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
            var AllObstacles = obstacleSpawner.allObstacles.Select(o => o.sphericalObstacle);
            var avoidance = SteerToAvoidObstacles(Globals.AvoidancePredictTimeMin, AllObstacles);

            // saved for annotation
            avoiding = avoidance == Vector3.Zero;

            steer = avoiding ? SteerForPursuit(seeker, maxPredictionTime) : avoidance;
        }
        else
        {
            ApplyBrakingForce(Globals.BrakingRate, elapsedTime);
        }

        ApplySteeringForce(steer, elapsedTime);

        // detect and record interceptions ("tags") of seeker
        var seekerToMeDist = Vector3.Distance(Position, seeker.Position);
        var sumOfRadii = Radius + seeker.Radius;
        if (seekerToMeDist < sumOfRadii)
        {
            if (seeker.State == SeekerState.Running) seeker.State = SeekerState.Tagged;

            // annotation:
            if (seeker.State == SeekerState.Tagged)
            {
                Godot.GD.Print("Seeker Tagged!");
            }
        }
    }
}
