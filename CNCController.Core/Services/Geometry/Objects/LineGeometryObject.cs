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

        public override double Length
        {
            get
            {
                return (Line.Direction.Length * Line.Origin.Distance(Line.Origin + Line.Direction));
            }
        }

        public Vector3d Start => Line.Origin;
        public Vector3d End => Line.Origin + Line.Direction;

        // Additional properties or methods can be defined here if needed
    }
}