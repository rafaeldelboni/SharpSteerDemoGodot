using SharpSteer2.Obstacles;

public partial class Obstacle : Node3D
{
    [Export] MeshInstance3D mesh;
    [Export] Material material;

    public SphericalObstacle Body { get; private set; }


    public Obstacle Configure(float radius, Vector3 position)
    {
        Position = position;
        Body = new(radius, position.ToNumerics());
        return this;
    }

    public override void _Ready()
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(material);

        CylinderMesh cylinder = new()
        {
            Height = 0.1f,
            TopRadius = Body.Radius,
            BottomRadius = Body.Radius,
        };

        mesh.Mesh = cylinder;
        mesh.MaterialOverride = material;
    }

    public float GetClearance(Vector3 point, float extraRadius = 0)
    {
        var distance = point.DistanceTo(Position);
        return distance - (extraRadius + Body.Radius);
    }

    public static float TestOneObstacleOverlap(float minClearance, float radius, Vector3 source, Vector3 target)
    {
        var clearance = source.DistanceTo(target) - radius;

        if (minClearance > clearance)
            minClearance = clearance;

        return minClearance;
    }
}
