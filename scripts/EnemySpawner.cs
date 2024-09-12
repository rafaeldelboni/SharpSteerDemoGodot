public partial class EnemySpawner : Node
{
    const int NumEnemies = 6;

    PackedScene enemyScene;
    Seeker seeker;
    ObstacleSpawner obstacleSpawner;

    public Enemy[] AllEnemies = [];

    public static EnemySpawner Instance { get; private set; }

    public override void _Ready()
    {
        enemyScene = GD.Load<PackedScene>("res://scenes/enemy.tscn");
        seeker = GetNode<Seeker>("%Seeker");
        obstacleSpawner = ObstacleSpawner.Instance;

        InitializeEnemies();

        foreach (var enemy in AllEnemies)
            AddChild(enemy);

        if (Instance != this) Instance?.QueueFree();
        Instance = this;
    }

    void InitializeEnemies()
    {
        AllEnemies = new Enemy[NumEnemies];
        for (var i = 0; i < AllEnemies.Length; i++)
            AllEnemies[i] = enemyScene.Instantiate<Enemy>().Configure(seeker);
    }
}
