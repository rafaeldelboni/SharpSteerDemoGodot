using SharpSteer2.Helpers;
using SharpSteer2.Obstacles;

public partial class ObstacleSpawner : Node
{
    PackedScene obstacleScene;
    [Export] int numObstacles = 100;
    [Export] float baseRadius = .10f;

    readonly List<Obstacle> obstacleNodes = [];
    readonly List<IObstacle> obstacleList = [];
    public IReadOnlyList<IObstacle> AllObstacles => obstacleList;

    int ObstacleCount => obstacleNodes.Count;

    public static ObstacleSpawner Instance { get; private set; }

    public override void _Ready()
    {
        obstacleScene = GD.Load<PackedScene>("res://scenes/obstacle.tscn");

        InitializeObstacles();

        if (Instance != this) Instance?.QueueFree();
        Instance = this;
    }

    // dynamic obstacle registry
    void InitializeObstacles()
    {
        Clear();

        for (var i = 0; i < numObstacles; i++)
            AddObstacle(GenerateObstacle());
    }

    void Clear()
    {
        obstacleNodes.Clear();
        obstacleList.Clear();
        this.FreeChildren();
    }

    Obstacle GenerateObstacle()
    {
        // todo use player radius
        const float requiredClearance = 0.5f * 4; // 2 x diameter

        var homeCenter = Globals.HomeBaseCenter.ToGodot();
        var radius = baseRadius - requiredClearance;

        // pick a random center and radius,
        // loop until no overlap with other obstacles and the home base
        float randomRadius;
        Vector3 randomCenter;
        float minClearance;

        do
        {
            randomRadius = RandomHelpers.Random(1.5f, 4);
            randomCenter = Vector3Helpers.RandomVectorOnUnitRadiusXZDisk().ToGodot() * Globals.MaxStartRadius * 1.1f;

            minClearance =
                Mathf.Min(
                    MinDistanceToPoint(randomCenter, randomRadius),
                    randomCenter.DistanceTo(homeCenter) - randomRadius + radius
                );
        } while (minClearance < requiredClearance);

        // add new non-overlapping obstacle to registry
        var obstacle = obstacleScene.Instantiate<Obstacle>().Configure(
            radius: randomRadius,
            position: randomCenter
        );

        return obstacle;
    }

    void AddObstacle(Obstacle obstacle)
    {
        obstacleNodes.Add(obstacle);
        AddChild(obstacle);
        obstacleList.Add(obstacle.SphericalObstacle);
    }

    void RemoveObstacle(Obstacle obstacle)
    {
        obstacleNodes.Remove(obstacle);
        obstacleList.Remove(obstacle.SphericalObstacle);
        RemoveChild(obstacle);
        obstacle.QueueFree();
    }

    public void RemoveLastObstacle()
    {
        if (ObstacleCount <= 0) return;
        RemoveObstacle(obstacleNodes[^1]);
    }

    public float MinDistanceToPoint(Vector3 point, float radius = 0) =>
        obstacleNodes.Min(t => t?.GetClearance(point, radius)) ?? float.MaxValue;
}
