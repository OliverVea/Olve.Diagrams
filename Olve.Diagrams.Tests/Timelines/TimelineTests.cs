using Olve.Diagrams.Timelines;
using Olve.Results.TUnit;

namespace Olve.Diagrams.Tests.Timelines;

public class TimelineTests
{
    [Test]
    public async Task Timeline_ValidInput_ReturnsExpectedOutput()
    {
        // Arrange
        TimelineEntry start = new("My Start", new DateTime(1996, 11, 10));
        TimelineEntry middle = new("My Middle", new DateTime(2025, 06, 07));
        TimelineEntry end = new("My End", new DateTime(2075, 06, 13));
        
        Timeline timeline = new("My Timeline",  start, middle, end);
        
        // Act
        var actualStart = timeline.Start;
        var actualEnd = timeline.End;
        
        // Assert
        await Assert.That(actualStart).SucceededAndValue().IsEqualTo(start.DateTime);
        await Assert.That(actualEnd).SucceededAndValue().IsEqualTo(end.DateTime);
    }
    
    [Test]
    public async Task Timeline_WithNoEntries_StartAndEndFail()
    {
        // Arrange
        Timeline timeline = new("My Timeline");
        
        // Act
        var actualStart = timeline.Start;
        var actualEnd = timeline.End;
        
        // Assert
        await Assert.That(actualStart).Failed();
        await Assert.That(actualEnd).Failed();
    }
    
    [Test]
    public async Task Timeline_WithNoEntriesButOverride_OverridesAreUsed()
    {
        // Arrange
        DateTime startOverride = new(1911, 3, 17);
        DateTime endOverride = new(1927, 12, 24);

        Timeline timeline = new("My Timeline")
        {
            StartOverride = startOverride,
            EndOverride = endOverride
        };
        
        // Act
        var actualStart = timeline.Start;
        var actualEnd = timeline.End;
        
        // Assert
        await Assert.That(actualStart).SucceededAndValue().IsEqualTo(startOverride);
        await Assert.That(actualEnd).SucceededAndValue().IsEqualTo(endOverride);
    }
    
    [Test]
    public async Task Timeline_WithEntriesAndOverride_OverridesAreUsed()
    {
        // Arrange
        DateTime startOverride = new(1911, 3, 17);
        DateTime endOverride = new(1927, 12, 24);
            
        TimelineEntry start = new("My Start", new DateTime(1996, 11, 10));
        TimelineEntry middle = new("My Middle", new DateTime(2025, 06, 07));
        TimelineEntry end = new("My End", new DateTime(2075, 06, 13));
        
        Timeline timeline = new("My Timeline",  start, middle, end)
        {
            StartOverride = startOverride,
            EndOverride = endOverride
        };
        
        // Act
        var actualStart = timeline.Start;
        var actualEnd = timeline.End;
        
        // Assert
        await Assert.That(actualStart).SucceededAndValue().IsEqualTo(startOverride);
        await Assert.That(actualEnd).SucceededAndValue().IsEqualTo(endOverride);
    }
}