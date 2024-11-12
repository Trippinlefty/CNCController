using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;
using CNCController.Core.Services.Geometry.Objects;
using g4;

namespace CNCController.Core.Services.Geometry;

public class GeometryService : IGeometryService
{
    public LineGeometryObject ConvertCommandToGeometry(IGCodeCommand command, Vector3d currentPosition)
    {
        if (command.CommandType != GCodeCommandType.Motion)
            throw new NotSupportedException("Unsupported G-code command type.");
        // Extract target coordinates from GCode command parameters
        var targetX = command.Parameters.TryGetValue("X", out var x) ? x : currentPosition.x;
        var targetY = command.Parameters.TryGetValue("Y", out var y) ? y : currentPosition.y;
        var targetZ = command.Parameters.TryGetValue("Z", out var z) ? z : currentPosition.z;

        var targetPosition = new Vector3d(targetX, targetY, targetZ);
        return new LineGeometryObject(new Line3d(currentPosition, targetPosition - currentPosition));

    }

    public List<Line3d> OptimizePath(List<Line3d> path)
    {
        throw new NotImplementedException();
    }

    public bool CheckForCollisions(Line3d path, AxisAlignedBox3d boundary)
    {
        throw new NotImplementedException();
    }

    public bool CheckForCollisions(GeometryObject path, GeometryObject boundary)
    {
        if (path is LineGeometryObject line1 && boundary is LineGeometryObject line2)
        {
            return DoLineSegmentsIntersect(line1.Line, line2.Line);
        }

        // Handle other geometry types as needed
        return false;
    }

    private bool DoLineSegmentsIntersect(Line3d line1, Line3d line2)
    {
        var p1 = line1.Origin;
        var q1 = line1.PointAt(1); // End point of line1
        var p2 = line2.Origin;
        var q2 = line2.PointAt(1); // End point of line2

        // Vector directions of the segments
        var d1 = q1 - p1;
        var d2 = q2 - p2;

        // Calculate the cross product to determine parallelism
        var crossD1D2 = d1.Cross(d2);
        if (crossD1D2.LengthSquared == 0) // Lines are parallel or collinear
        {
            // Check if the lines are collinear and overlap
            if ((p2 - p1).Cross(d1).LengthSquared == 0)
            {
                // Check if they overlap within the segment bounds
                return IsPointOnSegment(p1, q1, p2) || IsPointOnSegment(p1, q1, q2) ||
                       IsPointOnSegment(p2, q2, p1) || IsPointOnSegment(p2, q2, q1);
            }
            return false;
        }

        // Compute the closest approach of the two lines using vector projection
        var t1 = (p2 - p1).Dot(d2.Cross(crossD1D2)) / crossD1D2.LengthSquared;
        var t2 = (p2 - p1).Dot(d1.Cross(crossD1D2)) / crossD1D2.LengthSquared;

        // Calculate the points of closest approach on both segments
        var closestPointOnLine1 = p1 + t1 * d1;
        var closestPointOnLine2 = p2 + t2 * d2;

        // Check if the closest points are within the bounds of their respective segments
        var isOnSegment1 = t1 is >= 0 and <= 1;
        var isOnSegment2 = t2 is >= 0 and <= 1;

        return isOnSegment1 && isOnSegment2 && (closestPointOnLine1 - closestPointOnLine2).LengthSquared < 0.0001;
    }

    private bool IsPointOnSegment(Vector3d p, Vector3d q, Vector3d point)
    {
        // Check if the given point lies within the segment defined by p and q
        return (point - p).Dot(q - point) >= 0 && (point - q).Dot(p - point) >= 0;
    }


    Line3d IGeometryService.ConvertCommandToGeometry(GCodeCommand command, Vector3d currentPosition)
    {
        throw new NotImplementedException();
    }
}