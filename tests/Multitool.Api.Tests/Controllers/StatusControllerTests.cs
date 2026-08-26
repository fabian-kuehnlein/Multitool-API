using Microsoft.AspNetCore.Mvc;
using Multitool.Api.Controllers;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class StatusControllerTests
{
    public StatusControllerTests()
    {
    }

    private StatusController GetController()
    {
        return new StatusController();
    }

    // GET api/Status/live

    [Fact]
    public void Live_WhenCalled_ReturnsOkWithAliveStatus()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = controller.Live();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var payload = okResult.Value!;
        var statusProperty = payload.GetType().GetProperty("status");
        var timestampProperty = payload.GetType().GetProperty("timestamp");

        Assert.NotNull(statusProperty);
        Assert.NotNull(timestampProperty);

        var status = statusProperty!.GetValue(payload) as string;
        var timestamp = (DateTime)timestampProperty!.GetValue(payload)!;

        AssertEx.AreEqual("alive", status);
        AssertEx.CloseTo(timestamp, DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
