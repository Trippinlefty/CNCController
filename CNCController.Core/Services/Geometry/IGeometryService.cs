using CNCController.Core.Models;
using CNCController.Core.Services.Geometry;
using g4;

public interface IGeometryService
{
    Line3d ConvertCommandToGeometry(GCodeCommand command, Vector3d currentPosition);
    List<Line3d> OptimizePath(List<Line3d> path);
    bool CheckForCollisions(Line3d path, AxisAlignedBox3d boundary);
}