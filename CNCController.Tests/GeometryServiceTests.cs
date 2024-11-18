using Xunit;
using CNCController.Core.Services.Geometry;
using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;
using CNCController.Core.Services.Geometry.Objects;
using g4;
using Assert = Xunit.Assert;

namespace CNCController.Tests
{
    public class GeometryServiceTests
    {
        private readonly GeometryService _geometryService;

        public GeometryServiceTests()
        {
            _geometryService = new GeometryService();
        }

        [Fact]
        public void ConvertCommandToGeometry_CreatesExpectedLine()
        {
            // Arrange
            var currentPosition = new Vector3d(0, 0, 0);
            var command = new GCodeCommand(GCodeCommandType.Motion, new Dictionary<string, double>
            {
                { "X", 10 },
                { "Y", 20 },
                { "Z", 5 }
            })
            {
                CommandText = "G1 X10 Y20 Z5"
            };

            // Act
            var line = _geometryService.ConvertCommandToGeometry(command, currentPosition);

            // Assert
            Assert.Equal(currentPosition, line.Origin); // Ensure start point matches the current position
            Assert.Equal(new Vector3d(10, 20, 5), line.PointAt(1)); // Ensure the end point matches the command parameters
        }

        [Fact]
        public void CheckForCollisions_ReturnsTrue_WhenLinesIntersect()
        {
            // Arrange
            var line1 = new LineGeometryObject(new Line3d(new Vector3d(0, 0, 0), new Vector3d(5, 5, 0)));
            var line2 = new LineGeometryObject(new Line3d(new Vector3d(0, 5, 0), new Vector3d(5, 0, 0)));

            // Act
            var result = _geometryService.CheckForCollisions(line1, line2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForCollisions_ReturnsFalse_WhenLinesDoNotIntersect()
        {
            // Arrange
            var line1 = new LineGeometryObject(new Line3d(new Vector3d(0, 0, 0), new Vector3d(1, 1, 0)));
            var line2 = new LineGeometryObject(new Line3d(new Vector3d(3, 3, 0), new Vector3d(4, 4, 0)));

            // Act
            var result = _geometryService.CheckForCollisions(line1, line2);

            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void ConvertCommandToGeometry_ThrowsNotSupportedException_ForUnsupportedCommand()
        {
            // Arrange
            var currentPosition = new Vector3d(0, 0, 0);
            var command = new GCodeCommand(GCodeCommandType.SpindleControl, new Dictionary<string, double>());

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                _geometryService.ConvertCommandToGeometry(command, currentPosition)
            );

            Assert.Contains("Unsupported G-code command type", exception.Message);
        }
    }
}