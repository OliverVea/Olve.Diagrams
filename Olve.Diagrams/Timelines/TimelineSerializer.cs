using System.Text;
using Olve.Results;
using static Olve.Diagrams.Timelines.TimelineSerializationConstants;

namespace Olve.Diagrams.Timelines;

public class TimelineSerializer(NewlineProvider newlineProvider)
{
    public Result<string> Serialize(Timeline timeline)
    {
        StringBuilder sb = new();
        
        AddHeader(sb, timeline);
        AddEntries(sb, timeline);
        
        return sb.ToString();
    }

    private void AddHeader(StringBuilder sb, Timeline timeline)
    {
        AddHeaderEntry(sb, VersionHeaderKey, TimelineFormatVersion);
        AddHeaderEntry(sb, TitleHeaderKey, timeline.Title);
        AddHeaderEntry(sb, StartHeaderKey, timeline.StartOverride?.ToString(DateFormat));
        AddHeaderEntry(sb, EndHeaderKey, timeline.EndOverride?.ToString(DateFormat));
        
        sb.Append(SectionSeparator);
        sb.Append(newlineProvider.Newline);
    }

    private void AddHeaderEntry(StringBuilder sb, string headerKey, string? headerValue)
    {
        if (headerValue == null) return;
        
        sb.Append(headerKey);
        sb.Append(HeaderSeparator);
        sb.Append(' ');
        sb.Append(headerValue);
        sb.Append(newlineProvider.Newline);
    }

    private void AddEntries(StringBuilder sb, Timeline timeline)
    {
        foreach (var entry in timeline.Entries)
        {
            AddEntry(sb, entry);
        }
    }

    private void AddEntry(StringBuilder sb, TimelineEntry entry)
    {
        var direction = entry.Direction switch
        {
            TimelineEntryDirection.Up => DirectionUpKey,
            TimelineEntryDirection.Down => DirectionDownKey,
            _ => throw new ArgumentException($"Unknown entry direction: {entry.Direction}"),
        };

        sb.Append(direction);
        sb.Append(' ');
        sb.Append(entry.DateTime.ToString(DateFormat));
        sb.Append(' ');
        sb.Append(entry.Title);

        if (entry.Description is { } description)
        {
            sb.Append(HeaderSeparator);
            sb.Append(' ');
            sb.Append(description);
        }
        
        sb.Append(newlineProvider.Newline);
    }
}