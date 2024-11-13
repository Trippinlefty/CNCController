using g4;

namespace CNCController.Core.Services.Geometry.Objects
{
    public class CircleGeometryObject : GeometryObject
    {
        public Vector3d Center { get; }
        public double Radius { get; }

        public CircleGeometryObject(Vector3d center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public override double Length => 2 * Math.PI * Radius; // Circumference
        public override double Area => Math.PI * Radius * Radius; // Area of the circle
    }
}