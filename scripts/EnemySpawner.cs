public partial class EnemySpawner : Node
{
    [Export] int numEnemies = 6;
    [Export] PackedScene enemyScene;
    [Export] Seeker seeker;

    public Enemy[] AllEnemies = [];

    public static EnemySpawner Instance { get; private set; }

    public override void _Ready()
    {
        ArgumentNullException.ThrowIfNull(seeker);
        ArgumentNullException.ThrowIfNull(enemyScene);

        InitializeEnemies();

        if (Instance != this) Instance?.QueueFree();
        Instance = this;
    }

    void InitializeEnemies()
    {
        AllEnemies = new Enemy[numEnemies];
        this.FreeChildren();

        for (var i = 0; i < AllEnemies.Length; i++)
        {
            var enemy = enemyScene.Instantiate<Enemy>();
            enemy.Seek(seeker);
            AllEnemies[i] = enemy;
            AddChild(enemy);
        }
    }
}
