public static partial class Extensions
{
    public static System.Numerics.Vector3 ToNumerics(this Godot.Vector3 vector)
        => new(vector.X, vector.Y, vector.Z);

    public static Godot.Vector3 ToGodot(this System.Numerics.Vector3 vector)
        => new(vector.X, vector.Y, vector.Z);

    public static System.Numerics.Vector3 ToNumerics(this Godot.Color color)
        => new(color.R, color.G, color.B);
}
