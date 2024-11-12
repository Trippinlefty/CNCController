using CNCController.Core.Models;
using CNCController.Core.Services.GCodeProcessing;
using CNCController.Core.Services.Geometry;
using CNCController.Core.Services.Geometry.Objects;
using g4;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CNCController.Tests;

public class GeometryServiceTests
{
    private readonly GeometryService _geometryService;

    public GeometryServiceTests()
    {
        // Initialize GeometryService in the constructor for xUnit
        _geometryService = new GeometryService();
    }

    [Fact]
    public void ConvertCommandToGeometry_WithMotionCommand_ReturnsLineGeometryObject()
    {
        // Arrange
        var currentPosition = new Vector3d(0, 0, 0);
            
            
        var commandMock = new Mock<IGCodeCommand>();
        commandMock.SetupGet(cmd => cmd.CommandType).Returns(GCodeCommandType.Motion);
        commandMock.SetupGet(cmd => cmd.Parameters).Returns(new Dictionary<string, double>
        {
            { "X", 10 },
            { "Y", 15 },
            { "Z", 5 }
        });

        // Act
        var geometryObject = _geometryService.ConvertCommandToGeometry(commandMock.Object, currentPosition);

        // Assert
        Assert.IsType<LineGeometryObject>(geometryObject);
        if (geometryObject != null)
        {
            var line = geometryObject;
            Assert.Equal(new Vector3d(0, 0, 0), line.Start);
            Assert.Equal(new Vector3d(10, 15, 5), line.End);
        }
    }

    [Fact]
    public void CheckForCollisions_WithIntersectingLines_ReturnsTrue()
    {
        // Arrange
        var line1 = new LineGeometryObject(new Line3d(new Vector3d(0, 0, 0), new Vector3d(10, 10, 0) - new Vector3d(0, 0, 0)));
        var line2 = new LineGeometryObject(new Line3d(new Vector3d(0, 10, 0), new Vector3d(10, 0, 0) - new Vector3d(0, 10, 0)));

        // Act
        var result = _geometryService.CheckForCollisions(line1, line2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CheckForCollisions_WithNonIntersectingLines_ReturnsFalse()
    {
        // Arrange
        var line1 = new LineGeometryObject(new Line3d(new Vector3d(0, 0, 0), new Vector3d(5, 5, 0) - new Vector3d(0, 0, 0)));
        var line2 = new LineGeometryObject(new Line3d(new Vector3d(10, 10, 0), new Vector3d(15, 15, 0) - new Vector3d(10, 10, 0)));

        // Act
        var result = _geometryService.CheckForCollisions(line1, line2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConvertCommandToGeometry_WithIncompleteParameters_UsesCurrentPosition()
    {
        // Arrange
        var currentPosition = new Vector3d(5, 5, 5);

        var command = new GCodeCommand(
            GCodeCommandType.Motion,
            new Dictionary<string, double> { { "Y", 10 } } // Only Y provided
        );

        // Act
        var geometryObject = _geometryService.ConvertCommandToGeometry(command, currentPosition);

        // Assert
        Assert.IsType<LineGeometryObject>(geometryObject);
        if (geometryObject != null)
        {
            var line = geometryObject;
            Assert.Equal(currentPosition, line.Start);
            Assert.Equal(new Vector3d(5, 10, 5), line.End); // X and Z should remain as in currentPosition
        }
    }
}