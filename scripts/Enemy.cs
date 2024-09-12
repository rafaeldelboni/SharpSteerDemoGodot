public partial class Enemy : Node3D
{
    public EnemyVehicle Vehicle;

    Seeker seeker;
    ObstacleSpawner obstacleSpawner;

    public Enemy Init(Seeker seeker, ObstacleSpawner obstacleSpawner)
    {
        this.seeker = seeker;
        this.obstacleSpawner = obstacleSpawner;
        return this;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Vehicle = new(seeker, obstacleSpawner);
        Vehicle.Reset();
        Vehicle.RandomizeStartingPositionAndHeading();
        Position = Vehicle.Position.ToGodot();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Vehicle.Update((float)delta);

        Position = Vehicle.Position.ToGodot();
        Rotation = new(0, Mathf.Lerp(Rotation.Y, Mathf.Atan2(-Vehicle.Velocity.X, -Vehicle.Velocity.Z), weight: 0.5f), 0);
    }
}
