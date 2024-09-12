public partial class Seeker : Node3D
{
    public SeekerVehicle Vehicle;

    public override void _Ready()
    {
        Vehicle = new();
        Vehicle.Reset();
        Vehicle.RandomizeStartingPositionAndHeading(ObstacleSpawner.Instance);
        Position = Vehicle.Position.ToGodot();
    }

    public override void _Process(double delta)
    {
        // do behavioral state transitions, as needed
        Vehicle.Update(
            (float)delta,
            (float)delta,
            ObstacleSpawner.Instance.AllObstacles,
            EnemySpawner.Instance.AllEnemies
        );

        Position = Vehicle.Position.ToGodot();
        Rotation = new(0, Mathf.Lerp(Rotation.Y, Mathf.Atan2(-Vehicle.Velocity.X, -Vehicle.Velocity.Z), weight: 0.5f),
            0);
    }
}
