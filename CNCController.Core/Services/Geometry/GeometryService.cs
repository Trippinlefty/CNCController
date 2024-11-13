using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;
using CNCController.Core.Services.Geometry.Objects;
using g4;

namespace CNCController.Core.Services.Geometry
{
    public class GeometryService : IGeometryService
    {
        public LineGeometryObject ConvertCommandToGeometry(IGCodeCommand command, Vector3d currentPosition)
        {
            if (command.CommandType != GCodeCommandType.Motion)
                throw new NotSupportedException("Unsupported G-code command type.");

            var targetX = command.Parameters.TryGetValue("X", out var x) ? x : currentPosition.x;
            var targetY = command.Parameters.TryGetValue("Y", out var y) ? y : currentPosition.y;
            var targetZ = command.Parameters.TryGetValue("Z", out var z) ? z : currentPosition.z;

            var targetPosition = new Vector3d(targetX, targetY, targetZ);
            return new LineGeometryObject(new Line3d(currentPosition, targetPosition - currentPosition));
        }

        public Line3d ConvertCommandToGeometry(GCodeCommand command, Vector3d currentPosition)
        {
            throw new NotImplementedException();
        }

        public List<Line3d> OptimizePath(List<Line3d> path)
        {
            // Placeholder: Implement path reordering, smoothing, etc.
            return path;
        }

        public bool CheckForCollisions(GeometryObject path, GeometryObject boundary)
        {
            if (path is LineGeometryObject line1 && boundary is LineGeometryObject line2)
            {
                return DoLineSegmentsIntersect(line1.Line, line2.Line);
            }
            else if (path is LineGeometryObject line && boundary is AxisAlignedBox3d)
            {
                // Cast boundary explicitly to AxisAlignedBox3d
                AxisAlignedBox3d box = (AxisAlignedBox3d)(object)boundary;
                return LineIntersectsBox(line.Line, box);
            }
            else if (path is CircleGeometryObject circle && boundary is AxisAlignedBox3d)
            {
                // Cast boundary explicitly to AxisAlignedBox3d
                AxisAlignedBox3d boundingBox = (AxisAlignedBox3d)(object)boundary;
                return CheckCircleBoxCollision(circle, boundingBox);
            }
            return false;
        }

        private bool DoLineSegmentsIntersect(Line3d line1, Line3d line2)
        {
            var p1 = line1.Origin;
            var q1 = line1.PointAt(1);
            var p2 = line2.Origin;
            var q2 = line2.PointAt(1);

            var d1 = q1 - p1;
            var d2 = q2 - p2;

            var crossD1D2 = d1.Cross(d2);
            if (crossD1D2.LengthSquared == 0)
            {
                if ((p2 - p1).Cross(d1).LengthSquared == 0)
                {
                    return IsPointOnSegment(p1, q1, p2) || IsPointOnSegment(p1, q1, q2) ||
                           IsPointOnSegment(p2, q2, p1) || IsPointOnSegment(p2, q2, q1);
                }
                return false;
            }

            var t1 = (p2 - p1).Dot(d2.Cross(crossD1D2)) / crossD1D2.LengthSquared;
            var t2 = (p2 - p1).Dot(d1.Cross(crossD1D2)) / crossD1D2.LengthSquared;

            var isOnSegment1 = t1 is >= 0 and <= 1;
            var isOnSegment2 = t2 is >= 0 and <= 1;

            return isOnSegment1 && isOnSegment2 && (p1 + t1 * d1 - (p2 + t2 * d2)).LengthSquared < 0.0001;
        }

        private bool LineIntersectsBox(Line3d line, AxisAlignedBox3d box)
        {
            // Implement actual line-box intersection logic if necessary
            return true;
        }

        private bool CheckCircleBoxCollision(CircleGeometryObject circle, AxisAlignedBox3d box)
        {
            var closestPoint = ClosestPointOnBox(circle.Center, box);
            var distanceSquared = (closestPoint - circle.Center).LengthSquared;
            return distanceSquared <= circle.Radius * circle.Radius;
        }

        private Vector3d ClosestPointOnBox(Vector3d point, AxisAlignedBox3d box)
        {
            double x = Math.Max(box.Min.x, Math.Min(point.x, box.Max.x));
            double y = Math.Max(box.Min.y, Math.Min(point.y, box.Max.y));
            double z = Math.Max(box.Min.z, Math.Min(point.z, box.Max.z));

            return new Vector3d(x, y, z);
        }

        private bool IsPointOnSegment(Vector3d p, Vector3d q, Vector3d point)
        {
            return (point - p).Dot(q - point) >= 0 && (point - q).Dot(p - point) >= 0;
        }
    }
}
