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
}
