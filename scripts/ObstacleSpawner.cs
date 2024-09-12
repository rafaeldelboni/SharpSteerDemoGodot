using SharpSteer2.Helpers;
using SharpSteer2.Obstacles;

public partial class ObstacleSpawner : Node
{
    const int NumObstacles = 100;

    PackedScene obstacleScene;

    float baseRadius = 1.5f;
    readonly List<Obstacle> obstacleNodes = [];
    public IEnumerable<IObstacle> AllObstacles => obstacleNodes.Select(o => o.SphericalObstacle);

    int obstacleCount = -1;

    public static ObstacleSpawner Instance { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        obstacleScene = GD.Load<PackedScene>("res://scenes/obstacle.tscn");

        InitializeObstacles(0.10f);

        foreach (var obstacles in obstacleNodes)
            AddChild(obstacles);

        if (Instance != this)
            Instance?.QueueFree();

        Instance = this;
    }

    // dynamic obstacle registry
    public void InitializeObstacles(float radius)
    {
        obstacleNodes.Clear();

        // start with 40% of possible obstacles
        if (obstacleCount is not -1)
            return;

        obstacleCount = 0;

        for (var i = 0; i < NumObstacles; i++)
            AddOneObstacle(radius);
    }

    void AddOneObstacle(float radius)
    {
        // pick a random center and radius,
        // loop until no overlap with other obstacles and the home base
        float r;
        Vector3 c;
        float minClearance;
        // todo use player radius
        var requiredClearance = 0.5f * 4; // 2 x diameter
        do
        {
            r = RandomHelpers.Random(1.5f, 4);
            c = Vector3Helpers.RandomVectorOnUnitRadiusXZDisk().ToGodot() * Globals.MaxStartRadius * 1.1f;
            minClearance = obstacleNodes.Aggregate(float.MaxValue,
                (current, t) => TestOneObstacleOverlap(current, r + t.Radius, c, t.Position));

            minClearance = TestOneObstacleOverlap(minClearance, r + radius - requiredClearance, c,
                Globals.HomeBaseCenter.ToGodot());
        } while (minClearance < requiredClearance);

        // add new non-overlapping obstacle to registry
        var obstacle = obstacleScene.Instantiate<Obstacle>();
        obstacle.Radius = r;
        obstacle.Position = c;
        obstacleNodes.Add(obstacle);
        obstacleCount++;
    }

    void RemoveOneObstacle()
    {
        if (obstacleCount <= 0)
            return;

        obstacleCount--;
        obstacleNodes.RemoveAt(obstacleCount);
    }

    public float MinDistanceToObstacle(Vector3 point) =>
        obstacleNodes.Aggregate(
            float.MaxValue,
            (current, t) => TestOneObstacleOverlap(current, t.Radius, point, t.Position));

    static float TestOneObstacleOverlap(float minClearance, float radius, Vector3 source, Vector3 target)
    {
        var distance = source.DistanceTo(target);
        var clearance = distance - radius;

        if (minClearance > clearance)
            minClearance = clearance;

        return minClearance;
    }
}
