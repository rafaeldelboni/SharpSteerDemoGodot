using SharpSteer2.Helpers;
using SharpSteer2.Obstacles;

public partial class ObstacleSpawner : Node
{
    const int NumObstacles = 100;

    PackedScene obstacleScene;

    float baseRadius = 1.5f;

    readonly List<Obstacle> obstacleNodes = [];
    public IEnumerable<IObstacle> AllObstacles => obstacleNodes.Select(o => o.SphericalObstacle);

    int ObstacleCount => obstacleNodes.Count;

    public static ObstacleSpawner Instance { get; private set; }

    public override void _Ready()
    {
        obstacleScene = GD.Load<PackedScene>("res://scenes/obstacle.tscn");

        InitializeObstacles(0.10f);

        if (Instance != this) Instance?.QueueFree();
        Instance = this;
    }

    // dynamic obstacle registry
    void InitializeObstacles(float radius)
    {
        obstacleNodes.Clear();
        this.FreeChildren();

        for (var i = 0; i < NumObstacles; i++)
            AddObstacle(radius);
    }

    void AddObstacle(float radius)
    {
        // pick a random center and radius,
        // loop until no overlap with other obstacles and the home base
        float randomRadius;
        Vector3 randomCenter;
        float minClearance;

        // todo use player radius
        const float requiredClearance = 0.5f * 4; // 2 x diameter

        do
        {
            randomRadius = RandomHelpers.Random(1.5f, 4);
            randomCenter = Vector3Helpers.RandomVectorOnUnitRadiusXZDisk().ToGodot() * Globals.MaxStartRadius * 1.1f;

            minClearance = TestOneObstacleOverlap(
                MinDistanceToObstacle(randomCenter, randomRadius),
                randomRadius + radius - requiredClearance,
                randomCenter,
                Globals.HomeBaseCenter.ToGodot()
            );
        } while (minClearance < requiredClearance);

        // add new non-overlapping obstacle to registry
        var obstacle = obstacleScene.Instantiate<Obstacle>().Configure(
            radius: randomRadius,
            position: randomCenter
        );

        obstacleNodes.Add(obstacle);
        AddChild(obstacle);
    }

    void RemoveLastObstacle()
    {
        if (ObstacleCount <= 0) return;
        var obstacle = obstacleNodes[^1];
        RemoveChild(obstacle);
        obstacle.QueueFree();
        obstacleNodes.Remove(obstacle);
    }

    public float MinDistanceToObstacle(Vector3 point, float radius = 0) =>
        obstacleNodes.Aggregate(
            float.MaxValue,
            (current, t) => TestOneObstacleOverlap(current, t.Radius + radius, point, t.Position));

    static float TestOneObstacleOverlap(float minClearance, float radius, Vector3 source, Vector3 target)
    {
        var distance = source.DistanceTo(target);
        var clearance = distance - radius;

        if (minClearance > clearance)
            minClearance = clearance;

        return minClearance;
    }
}
