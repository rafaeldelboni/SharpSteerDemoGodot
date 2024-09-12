using System.Collections.Generic;
using System.Linq;
using Godot;
using SharpSteer2.Helpers;

public partial class ObstacleSpawner : Node
{
	static int numObstacles = 100;

	static PackedScene obstacleScene;

	float baseRadius = 1.5f;
	public List<Obstacle> allObstacles;
	protected static int obstacleCount = -1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		obstacleScene = GD.Load<PackedScene>($"res://scenes/obstacle.tscn");

		InitializeObstacles(0.10f, numObstacles);

		foreach (var obstacles in allObstacles)
			AddChild(obstacles);
	}

	// dynamic obstacle registry
	public void InitializeObstacles(float radius, int obstacles)
	{
		allObstacles = new();
		// start with 40% of possible obstacles
		if (obstacleCount == -1)
		{
			obstacleCount = 0;
			for (var i = 0; i < obstacles; i++)
				AddOneObstacle(radius);
		}
	}

	public void AddOneObstacle(float radius)
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
			minClearance = allObstacles.Aggregate(float.MaxValue, (current, t) => TestOneObstacleOverlap(current, r, t.Radius, c, t.Position));

			minClearance = TestOneObstacleOverlap(minClearance, r, radius - requiredClearance, c, Globals.HomeBaseCenter.ToGodot());
		}
		while (minClearance < requiredClearance);

		// add new non-overlapping obstacle to registry
		var obstacle = obstacleScene.Instantiate<Obstacle>();
		obstacle.Radius = r;
		obstacle.Position = c;
		allObstacles.Add(obstacle);
		obstacleCount++;
	}

	public void RemoveOneObstacle()
	{
		if (obstacleCount <= 0)
			return;

		obstacleCount--;
		allObstacles.RemoveAt(obstacleCount);
	}

	public float MinDistanceToObstacle(Vector3 point)
	{
		const float r = 0;
		var c = point;
		return allObstacles.Aggregate(float.MaxValue, (current, t) => TestOneObstacleOverlap(current, r, t.Radius, c, t.Position));
	}

	public float TestOneObstacleOverlap(float minClearance, float r, float radius, Vector3 c, Vector3 center)
	{
		var d = c.DistanceTo(center);
		var clearance = d - (r + radius);
		if (minClearance > clearance) minClearance = clearance;
		return minClearance;
	}
}
