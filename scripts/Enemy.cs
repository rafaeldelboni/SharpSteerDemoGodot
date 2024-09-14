public partial class Enemy : Node3D
{
    public EnemyVehicle Vehicle { get; private set; }

    Seeker seeker;

    public void Seek(Seeker seeker)
    {
        this.seeker = seeker;

        if (Vehicle is not null)
            Vehicle.Seeker = seeker.Vehicle;
    }

    public override void _Ready()
    {
        Vehicle = new(seeker.Vehicle);
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

        var yaw = Mathf.Lerp(
            Rotation.Y,
            Mathf.Atan2(-Vehicle.Velocity.X, -Vehicle.Velocity.Z),
            weight: 0.5f
        );

        Rotation = yaw * Vector3.Up;
    }
}
