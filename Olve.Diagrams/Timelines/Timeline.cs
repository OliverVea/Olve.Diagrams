using System.Diagnostics;
using Olve.Results;

namespace Olve.Diagrams.Timelines;

[DebuggerDisplay("{ToString()}")]
public class Timeline(string title, params IEnumerable<TimelineEntry> entries) : IEquatable<Timeline>
{
    public string Title { get; } = title;
    public IReadOnlyList<TimelineEntry> Entries { get; } = entries.ToList();
    public DateTime? StartOverride { get; init; }
    public DateTime? EndOverride { get; init; }
    
    public Result<DateTime> Start => StartOverride ?? GetStart();
    public Result<DateTime> End => EndOverride ?? GetEnd(); 

    private Result<DateTime> GetStart() => Entries.Count > 0
        ? Entries.Min(e => e.DateTime)
        : new ResultProblem("No entries in timeline to determine start date.");
    
    private Result<DateTime> GetEnd() => Entries.Count > 0
        ? Entries.Max(e => e.DateTime)
        : new ResultProblem("No entries in timeline to determine end date.");
    
    public override string ToString()
    {
        var start = Start.TryPickValue(out var startDate)
            ? startDate.ToString("yyyy-MM-dd")
            : "unknown";
        
        var end = End.TryPickValue(out var endDate)
            ? endDate.ToString("yyyy-MM-dd")
            : "unknown";
        
        return $"{Title} ({start} - {end}) {Entries.Count} entries";
    }

    public bool Equals(Timeline? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Entries.Count != other.Entries.Count) return false;
        
        foreach (var entrySet in Entries.Zip(other.Entries))
        {
            var (thisEntry, otherEntry) = entrySet;
            if (thisEntry != otherEntry) return false;
        }
        
        return Title == other.Title && Nullable.Equals(StartOverride, other.StartOverride) && Nullable.Equals(EndOverride, other.EndOverride);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Timeline)obj);
    }

    public override int GetHashCode()
    {
        var entryHashCode = Entries.Aggregate(17, (current, entry) => current * 31 + entry.GetHashCode());
        return HashCode.Combine(Title, entryHashCode);
    }
}