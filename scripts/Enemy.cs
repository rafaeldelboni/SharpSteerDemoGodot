public partial class Enemy : Node3D
{
    public EnemyVehicle Vehicle;

    Seeker seeker;

    public Enemy Configure(Seeker seeker)
    {
        this.seeker = seeker;
        return this;
    }

    public override void _Ready()
    {
        Vehicle = new(seeker);
        Vehicle.Reset();
        Vehicle.RandomizeStartingPositionAndHeading(ObstacleSpawner.Instance);
        Position = Vehicle.Position.ToGodot();
    }

    public override void _Process(double delta)
    {
        Vehicle.Update(
            (float)delta,
            ObstacleSpawner.Instance.AllObstacles
        );

        Position = Vehicle.Position.ToGodot();
        var yaw = Mathf.Lerp(Rotation.Y, Mathf.Atan2(-Vehicle.Velocity.X, -Vehicle.Velocity.Z), weight: 0.5f);
        Rotation = new(0, yaw, 0);
    }
}
