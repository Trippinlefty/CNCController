using CNCController.Core.Services.Geometry;
using g4;

public class CircleGeometryObject : GeometryObject
{
    public Vector3d Center { get; }
    public double Radius { get; }

    public CircleGeometryObject(Vector3d center, double radius)
    {
        Center = center;
        Radius = radius;
    }

    public override double Length => 2 * Math.PI * Radius; // Circumference as an example

    // Additional properties or methods specific to circles can be added here
}