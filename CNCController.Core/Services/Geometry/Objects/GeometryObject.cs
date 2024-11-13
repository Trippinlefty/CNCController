namespace CNCController.Core.Services.Geometry.Objects
{
    public abstract class GeometryObject
    {
        public abstract double Length { get; }
        public virtual double Area => 0; // Default area for non-area-based objects
    }
}