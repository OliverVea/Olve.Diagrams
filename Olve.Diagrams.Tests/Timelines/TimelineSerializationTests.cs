using Olve.Diagrams.Timelines;
using Olve.Results.TUnit;

namespace Olve.Diagrams.Tests.Timelines;

public class TimelineSerializationTests
{
    private static readonly NewlineProvider NewlineProvider = new();
    private static readonly TimelineSerializer TimelineSerializer = new(NewlineProvider);
    private static readonly TimelineDeserializer TimelineDeserializer = new(NewlineProvider);

    public static IEnumerable<Func<TestDataEntry<Timeline>>> GetTimelines()
    {
        yield return () =>
        {
            Timeline timeline = new("Compact History",
                new TimelineEntry("Born", new DateTime(2000, 1, 1), "A cold winter day"),
                new TimelineEntry("Learned to crawl", new DateTime(2000, 1, 2), "Started moving", TimelineEntryDirection.Up),
                new TimelineEntry("First birthday", new DateTime(2000, 1, 5)),
                new TimelineEntry("First word", new DateTime(2000, 1, 3), "Said 'mama'"),
                new TimelineEntry("First steps", new DateTime(2000, 1, 4), "Wobbly but determined", TimelineEntryDirection.Up));

            return new TestDataEntry<Timeline>("01_basic-dense", timeline);
        };

        yield return () =>
        {
            Timeline timeline = new("Sparse Timeline",
                new TimelineEntry("Declaration", new DateTime(1850, 7, 4), "Signed with great fanfare"),
                new TimelineEntry("Industrialization", new DateTime(1900, 1, 1)),
                new TimelineEntry("Millennium Eve", new DateTime(1999, 12, 31), "Huge global celebrations"))
            {
                StartOverride = new DateTime(1800, 1, 1),
                EndOverride = new DateTime(2000, 1, 1)
            };

            return new TestDataEntry<Timeline>("02_sparse-events", timeline);
        };

        yield return () =>
        {
            Timeline timeline = new("Alternating Events",
                new TimelineEntry("Start", new DateTime(1950, 1, 1), "A new era begins"),
                new TimelineEntry("Rise", new DateTime(1960, 1, 1), "Innovation booms", TimelineEntryDirection.Up),
                new TimelineEntry("End", new DateTime(2000, 1, 1), "Turn of the millennium", TimelineEntryDirection.Up),
                new TimelineEntry("Fall", new DateTime(1970, 1, 1), "Economic crash"),
                new TimelineEntry("Rebirth", new DateTime(1980, 1, 1), Direction: TimelineEntryDirection.Up),
                new TimelineEntry("Peak", new DateTime(1990, 1, 1)))
            {
                StartOverride = new DateTime(1950, 1, 1),
                EndOverride = new DateTime(2000, 1, 1)
            };

            return new TestDataEntry<Timeline>("03_alternating-markers", timeline);
        };

        yield return () =>
        {
            Timeline timeline = new("Single Day Drama",
                new TimelineEntry("Morning Rush", new DateTime(2024, 12, 25), "Forgot the presents"),
                new TimelineEntry("Lunch Talk", new DateTime(2024, 12, 25), "Family reunion", TimelineEntryDirection.Up),
                new TimelineEntry("Evening Calm", new DateTime(2024, 12, 25)))
            {
                StartOverride = new DateTime(2024, 12, 25),
                EndOverride = new DateTime(2024, 12, 25)
            };

            return new TestDataEntry<Timeline>("04_same-day-multiple", timeline);
        };

        yield return () =>
        {
            Timeline timeline = new("Critical Moments",
                new TimelineEntry("Discovery", new DateTime(2120, 5, 10), "First contact with alien life", TimelineEntryDirection.Up),
                new TimelineEntry("Turning Point", new DateTime(2150, 10, 22), Direction: TimelineEntryDirection.Up),
                new TimelineEntry("Final Moment", new DateTime(2199, 12, 31), "The end of the golden era", TimelineEntryDirection.Up))
            {
                StartOverride = new DateTime(2100, 1, 1),
                EndOverride = new DateTime(2200, 1, 1)
            };

            return new TestDataEntry<Timeline>("05_only-carets", timeline);
        };

        yield return () =>
        {
            Timeline timeline = new("Chaotic Input",
                new TimelineEntry("Mid-Century", new DateTime(1950, 1, 1)),
                new TimelineEntry("Start", new DateTime(1900, 1, 1), "Where it all began", TimelineEntryDirection.Up),
                new TimelineEntry("End", new DateTime(2000, 1, 1), "Closing chapter"))
            {
                StartOverride = new DateTime(1900, 1, 1),
                EndOverride = new DateTime(2000, 1, 1)
            };

            return new TestDataEntry<Timeline>("06_out-of-order", timeline);
        };

        yield return () =>
        {
            Timeline timeline = new("Lone Marker",
                new TimelineEntry("The Day", new DateTime(2020, 1, 1), "An unforgettable moment"))
            {
                StartOverride = new DateTime(2020, 1, 1),
                EndOverride = new DateTime(2020, 1, 1)
            };

            return new TestDataEntry<Timeline>("07_one-event-only", timeline);
        };
    }

    public static IEnumerable<Func<TestDataEntry<string>>> GetTimelineStrings()
    {
        if (TestDataReader.GetTestDataFiles("timelines", "*.tl")
            .TryPickProblems(out var problems, out var timelineFiles))
        {
            throw new Exception(string.Join(", ", problems));
        }

        foreach (var timelineFile in timelineFiles)
        {
            yield return () => new TestDataEntry<string>(timelineFile.FileName, timelineFile.Content);
        }
    }

    public readonly record struct TestDataEntry<T>(string TestName, T TestData)
    {
        public override string ToString() => TestName;
    }
    
    [Test]
    [MethodDataSource(nameof(GetTimelines))]
    public async Task SerializationDeserialization(TestDataEntry<Timeline> testDataEntry)
    {
        var timeline = testDataEntry.TestData;
        var serializedResult = TimelineSerializer.Serialize(timeline);
        await Assert.That(serializedResult).Succeeded();

        var serialized = serializedResult.Value!;
        
        var deserializedResult = TimelineDeserializer.Deserialize(serialized);
        
        // Assert
        await Assert.That(deserializedResult).SucceededAndValue().IsEqualTo(timeline);
    }
    
    [Test]
    [MethodDataSource(nameof(GetTimelineStrings))]
    public async Task DeserializationSerialization(TestDataEntry<string> testDataEntry)
    {
        var timelineString = testDataEntry.TestData;
        var deserializedResult = TimelineDeserializer.Deserialize(timelineString);
        await Assert.That(deserializedResult).Succeeded();

        var deserialized = deserializedResult.Value!;
        
        var serializedResult = TimelineSerializer.Serialize(deserialized);
        
        // Assert
        await Assert.That(serializedResult).Succeeded();

        var serializedValue = serializedResult.Value!;
        await Assert.That(serializedValue.Trim()).IsEqualTo(timelineString.Trim());
    }
}