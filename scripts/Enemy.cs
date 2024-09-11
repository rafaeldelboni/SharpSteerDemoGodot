using Godot;

public partial class Enemy : Node3D
{
    public EnemyVehicle vehicle;
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
        vehicle = new(seeker, obstacleSpawner);
        vehicle.Reset();
        vehicle.RandomizeStartingPositionAndHeading();
        Position = vehicle.Position.ToGodot();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        vehicle.Update((float)delta, (float)delta);

        Position = vehicle.Position.ToGodot();
        Rotation = new(0, Mathf.Lerp(Rotation.Y, Mathf.Atan2(-vehicle.Velocity.X, -vehicle.Velocity.Z), weight: 0.5f), 0);
    }
}
