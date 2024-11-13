using CNCController.Core.Models;
using CNCController.Core.Services.Geometry.Objects;
using g4;

namespace CNCController.Core.Services.Geometry;

public interface IGeometryService
{
    Line3d ConvertCommandToGeometry(GCodeCommand command, Vector3d currentPosition);
    List<Line3d> OptimizePath(List<Line3d> path);
    // Single method for collision detection with GeometryObject types
    bool CheckForCollisions(GeometryObject path, GeometryObject boundary);
}