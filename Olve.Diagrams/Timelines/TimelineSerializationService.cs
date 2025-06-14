using Olve.Results;

namespace Olve.Diagrams.Timelines;

internal class TimelineSerializationService(
    TimelineSerializer timelineSerializer,
    TimelineDeserializer timelineDeserializer) : ITimelineSerializer
{
    public Result<string> Serialize(Timeline timeline)
    {
        return timelineSerializer.Serialize(timeline);
    }

    public Result<Timeline> Deserialize(string input)
    {
        return timelineDeserializer.Deserialize(input);
    }
}