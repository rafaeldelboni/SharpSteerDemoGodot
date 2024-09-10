using Godot;
using SharpSteer2.Obstacles;

public partial class Obstacle : Node3D
{
    [Export] public float Radius;
    public SphericalObstacle sphericalObstacle;

    MeshInstance3D mesh;

    public override void _Ready()
    {
        var material = GD.Load<Material>($"res://assets/material/obstacle_material.tres");
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        var cylinder = new CylinderMesh();
        cylinder.Height = 0.1f;
        cylinder.TopRadius = Radius;
        cylinder.BottomRadius = Radius;
        mesh.Mesh = cylinder;
        mesh.MaterialOverride = material;
        sphericalObstacle = new(Radius, Position.ToNumerics());
    }
}
