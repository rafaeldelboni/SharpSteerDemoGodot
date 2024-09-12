public static class Extensions
{
    public static NVector3 ToNumerics(this Vector3 vector)
        => new(vector.X, vector.Y, vector.Z);

    public static Vector3 ToGodot(this NVector3 vector)
        => new(vector.X, vector.Y, vector.Z);

    public static NVector3 ToNumerics(this Color color)
        => new(color.R, color.G, color.B);
}
