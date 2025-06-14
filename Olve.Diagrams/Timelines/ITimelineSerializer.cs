using Olve.Results;

namespace Olve.Diagrams.Timelines;

public interface ITimelineSerializer
{
    /// <summary>
    /// Serializes the timeline to a string.
    /// </summary>
    /// <param name="timeline">The timeline to serialize.</param>
    /// <returns>A string representation of the timeline.</returns>
    Result<string> Serialize(Timeline timeline);

    /// <summary>
    /// Deserializes a string into a timeline.
    /// </summary>
    /// <param name="input">The string input to deserialize.</param>
    /// <returns>A Result containing the deserialized Timeline or an error.</returns>
    Result<Timeline> Deserialize(string input);
}