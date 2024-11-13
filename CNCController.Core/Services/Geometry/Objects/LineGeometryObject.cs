using g4;

namespace CNCController.Core.Services.Geometry.Objects
{
    public class LineGeometryObject : GeometryObject
    {
        public Line3d Line { get; }

        public LineGeometryObject(Line3d line)
        {
            Line = line;
        }

        // Calculate length using the distance between start and end points
        public override double Length => (End - Start).Length;

        public Vector3d Start => Line.Origin;
        public Vector3d End => Line.PointAt(1); // Assuming PointAt(1) gives the end point

        public Vector3d Midpoint => Start + 0.5 * (End - Start);
    }
}