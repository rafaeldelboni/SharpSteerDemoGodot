using SharpSteer2.Obstacles;

public partial class Obstacle : Node3D
{
    [Export] public float Radius;
    public SphericalObstacle SphericalObstacle;

    MeshInstance3D mesh;

    public Obstacle Configure(float radius, Vector3 position)
    {
        Radius = radius;
        Position = position;
        return this;
    }

    public override void _Ready()
    {
        CylinderMesh cylinder = new()
        {
            Height = 0.1f,
            TopRadius = Radius,
            BottomRadius = Radius,
        };

        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        mesh.Mesh = cylinder;
        mesh.MaterialOverride = GD.Load<Material>("res://assets/material/obstacle_material.tres");

        SphericalObstacle = new(Radius, Position.ToNumerics());
    }

    public float GetClearance(Vector3 point, float extraRadius = 0)
    {
        var distance = point.DistanceTo(Position);
        return distance - (extraRadius + SphericalObstacle.Radius);
    }

   public static float TestOneObstacleOverlap(float minClearance, float radius, Vector3 source, Vector3 target)
    {
        var clearance = source.DistanceTo(target) - radius;

        if (minClearance > clearance)
            minClearance = clearance;

        return minClearance;
    }

}
