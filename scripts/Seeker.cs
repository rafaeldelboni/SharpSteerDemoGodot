public partial class Seeker : Node3D
{
    public SeekerVehicle Vehicle;

    ObstacleSpawner obstacleSpawner;
    EnemySpawner enemySpawner;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        obstacleSpawner = GetNode<ObstacleSpawner>("%ObstacleSpawner");
        enemySpawner = GetNode<EnemySpawner>("%EnemySpawner");
        Vehicle = new(enemySpawner, obstacleSpawner);
        Vehicle.Reset();
        Vehicle.RandomizeStartingPositionAndHeading();
        Position = Vehicle.Position.ToGodot();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // do behavioral state transitions, as needed
        Vehicle.Update((float)delta, (float)delta);

        Position = Vehicle.Position.ToGodot();
        Rotation = new(0, Mathf.Lerp(Rotation.Y, Mathf.Atan2(-Vehicle.Velocity.X, -Vehicle.Velocity.Z), weight: 0.5f),
            0);
    }
}
