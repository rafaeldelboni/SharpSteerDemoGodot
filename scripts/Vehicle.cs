using SharpSteer2;
using SharpSteer2.Helpers;

public abstract class Vehicle : SimpleVehicle
{
    public override float MaxForce => 3;
    public override float MaxSpeed => 3;

    protected bool avoiding;

    protected Vehicle() => LocalReset();

    void LocalReset()
    {
        Speed = 3; // speed along Forward direction.
        avoiding = false; // not actively avoiding
    }


    public override void Reset()
    {
        base.Reset(); // reset the vehicle
        LocalReset();
    }

    public void RandomizeStartingPositionAndHeading(ObstacleSpawner obstacleSpawner)
    {
        // randomize position on a ring between inner and outer radii
        // centered around the home base
        var rRadius = RandomHelpers.Random(Globals.MinStartRadius, Globals.MaxStartRadius);
        var randomOnRing = Vector3Helpers.RandomUnitVectorOnXZPlane() * rRadius;
        Position = Globals.HomeBaseCenter + randomOnRing;

        // are we are too close to an obstacle?
        if (obstacleSpawner.MinDistanceToObstacle(Position.ToGodot()) < Radius * 5)
            // if so, retry the randomization (this recursive call may not return
            // if there is too little free space)
            RandomizeStartingPositionAndHeading(obstacleSpawner);
        else
            // otherwise, if the position is OK, randomize 2D heading
            RandomizeHeadingOnXZPlane();
    }
}
