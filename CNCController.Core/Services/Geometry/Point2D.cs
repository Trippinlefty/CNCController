using CNCController.Core.Services.Geometry.Objects;

namespace CNCController.Core.Services.Geometry
{
    public class Point2D : GeometryObject
    {
        public double X { get; }
        public double Y { get; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override double Length => 0; // A point has no length

        public double DistanceTo(Point2D other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public Point2D Midpoint(Point2D other)
        {
            return new Point2D((X + other.X) / 2, (Y + other.Y) / 2);
        }
    }
}