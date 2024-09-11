using System.Collections.Generic;
using SharpSteer2;
using SharpSteer2.Helpers;
using SharpSteer2.Obstacles;
using Vector3 = System.Numerics.Vector3;

public class Vehicle : SimpleVehicle
{
    public override float MaxForce => 3;
    public override float MaxSpeed => 3;
    public bool avoiding;

    protected ObstacleSpawner obstacleSpawner;

    // constructor
    public Vehicle(ObstacleSpawner obstacleSpawner) : base()
    {
        this.obstacleSpawner = obstacleSpawner;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();     // reset the vehicle
        Speed = 3;        // speed along Forward direction.
        avoiding = false; // not actively avoiding
    }

    public void RandomizeStartingPositionAndHeading()
    {
        // randomize position on a ring between inner and outer radii
        // centered around the home base
        var rRadius = RandomHelpers.Random(Globals.MinStartRadius, Globals.MaxStartRadius);
        var randomOnRing = Vector3Helpers.RandomUnitVectorOnXZPlane() * rRadius;
        Position = Globals.HomeBaseCenter + randomOnRing;

        // are we are too close to an obstacle?
        if (obstacleSpawner.MinDistanceToObstacle(Position.ToGodot()) < Radius * 5)
        {
            // if so, retry the randomization (this recursive call may not return
            // if there is too little free space)
            RandomizeStartingPositionAndHeading();
        }
        else
        {
            // otherwise, if the position is OK, randomize 2D heading
            RandomizeHeadingOnXZPlane();
        }
    }

    public new Vector3 SteerForFlee(Vector3 target)
    {
        return base.SteerForFlee(target);

    }

    public Vector3 SteerForPursuit(Vehicle vehicle, float maxPredictionTime)
    {
        return base.SteerForPursuit(vehicle, maxPredictionTime);
    }

    public Vector3 SteerToAvoidObstacles(float avoidancePredictTimeMin, IEnumerable<SphericalObstacle> sphericalObstacles)
    {
        return base.SteerToAvoidObstacles(avoidancePredictTimeMin, sphericalObstacles);
    }
}

