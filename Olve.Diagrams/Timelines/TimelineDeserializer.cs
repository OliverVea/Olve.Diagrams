using Olve.Results;
using static Olve.Diagrams.Timelines.TimelineSerializationConstants;

namespace Olve.Diagrams.Timelines;

public class NewlineProvider
{
    public string Newline { get; set; } = "\n";
}

public class TimelineDeserializer(NewlineProvider newlineProvider)
{
    private const StringSplitOptions TrimAndRemoveEmptyEntries =
        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
    
    private readonly record struct HeaderData(string Title, int HeaderLines, DateTime? StartOverride, DateTime? EndOverride);

    public Result<Timeline> Deserialize(string input)
    {
        var lines = input.Split(newlineProvider.Newline, TrimAndRemoveEmptyEntries);
        
        return Result.Chain(
            () => ParseHeader(lines),
            header => ParseTimeline(lines, header));
    }

    private Result<HeaderData> ParseHeader(string[] lines)
    {
        var headerEnd = -1;
        
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line == SectionSeparator)
            {
                headerEnd = i;
                break;
            }
        }

        if (headerEnd == -1)
        {
            return new ResultProblem("Did not find end of header '{0}'.", SectionSeparator);
        }

        string? version = null, title = null;
        DateTime? startOverride = null, endOverride = null;

        foreach (var line in lines[..headerEnd])
        {
            var iKeyEnd = line.IndexOf(HeaderSeparator);
            if (iKeyEnd == -1)
            {
                return new ResultProblem("Line '{0}' does not follow the 'key{1} value' format", line, HeaderSeparator);
            }

            var key = line[..iKeyEnd];
            var value = line[(iKeyEnd + 1)..].Trim();

            if (key == VersionHeaderKey) version = value;
            else if (key == TitleHeaderKey) title = value;
            else if (key == StartHeaderKey)
            {
                if (ParseDateTime(value).TryPickProblems(out var problems, out var dateTime))
                {
                    return problems.Prepend("Error parsing line '{0}'.", value);
                }

                startOverride = dateTime;
            }
            else if (key == EndHeaderKey)
            {
                if (ParseDateTime(value).TryPickProblems(out var problems, out var dateTime))
                {
                    return problems.Prepend("Error parsing line '{0}'.", value);
                }

                endOverride = dateTime;
            }
        }

        if (version == null)
        {
            return new ResultProblem("Header did not contain the file 'version' parameter.");
        }

        title ??= string.Empty;

        return new HeaderData(title, headerEnd, startOverride, endOverride);
    }

    private Result<Timeline> ParseTimeline(string[] lines, HeaderData headerData)
    {
        lines = lines[(headerData.HeaderLines + 1)..];
        List<TimelineEntry> entries = new(lines.Length);

        foreach (var l in lines)
        {
            ReadOnlySpan<char> line = l;
            if (ParseDirection(line[0]).TryPickProblems(out var problems, out var direction))
            {
                return problems.Prepend("Could not parse direction '{0}' of line '{1}'.", line[0], l);
            }

            line = line[1..].Trim();

            if (ParseDateTime(line[..DateFormat.Length]).TryPickProblems(out problems, out var dateTime))
            {
                return problems.Prepend("Could not parse date '{0} of line '{1}'.", line[..DateFormat.Length].ToString(), l);
            }

            line = line[DateFormat.Length..].Trim();

            var iTitleEnd = line.IndexOf(HeaderSeparator);

            var (title, description) = iTitleEnd == -1
                ? (line.ToString(), null)
                : (line[..iTitleEnd].Trim().ToString(), line[(iTitleEnd+1)..].Trim().ToString());

            TimelineEntry entry = new(title, dateTime, description, direction);
            entries.Add(entry);
        }
        
        return new Timeline(headerData.Title, entries)
        {
            StartOverride = headerData.StartOverride,
            EndOverride = headerData.EndOverride
        };
    }

    private Result<TimelineEntryDirection> ParseDirection(char ch)
    {
        return ch switch
        {
            DirectionUpKey => TimelineEntryDirection.Up,
            DirectionDownKey => TimelineEntryDirection.Down,
            _ => new ResultProblem("Could not parse '{0}' as timeline entry direction.", ch)
        };
    }

    private Result<DateTime> ParseDateTime(ReadOnlySpan<char> value)
    {
        if (DateTime.TryParse(value, out var start))
        {
            return start;
        }

        return new ResultProblem("Could not parse '{0}' as a date.", value.ToString());
    }
}