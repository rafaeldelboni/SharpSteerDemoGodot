using Godot;

public partial class EnemySpawner : Node
{
    static int numEnemies = 6;

    static PackedScene enemyScene;
    static Seeker seeker;
    static ObstacleSpawner obstacleSpawner;

    public Enemy[] allEnemies;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        enemyScene = GD.Load<PackedScene>($"res://scenes/enemy.tscn");
        seeker = GetNode<Seeker>("%Seeker");
        obstacleSpawner = GetNode<ObstacleSpawner>("%ObstacleSpawner");

        InitializeEnemies(numEnemies);

        foreach (var enemy in allEnemies)
            AddChild(enemy);
    }

    public void InitializeEnemies(int enemyCount)
    {
        allEnemies = new Enemy[enemyCount];
        for (var i = 0; i < allEnemies.Length; i++)
        {
            allEnemies[i] = enemyScene.Instantiate<Enemy>().Init(seeker, obstacleSpawner);
        }
    }
}
