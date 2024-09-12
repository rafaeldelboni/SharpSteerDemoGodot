public partial class EnemySpawner : Node
{
    const int NumEnemies = 6;

    PackedScene enemyScene;
    Seeker seeker;
    ObstacleSpawner obstacleSpawner;

    public Enemy[] AllEnemies = [];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        enemyScene = GD.Load<PackedScene>("res://scenes/enemy.tscn");
        seeker = GetNode<Seeker>("%Seeker");
        obstacleSpawner = GetNode<ObstacleSpawner>("%ObstacleSpawner");

        InitializeEnemies();

        foreach (var enemy in AllEnemies)
            AddChild(enemy);
    }

    void InitializeEnemies()
    {
        AllEnemies = new Enemy[NumEnemies];
        for (var i = 0; i < AllEnemies.Length; i++)
            AllEnemies[i] = enemyScene.Instantiate<Enemy>().Init(seeker, obstacleSpawner);
    }
}
