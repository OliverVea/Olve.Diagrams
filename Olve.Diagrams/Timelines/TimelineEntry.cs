namespace Olve.Diagrams.Timelines;

public readonly record struct TimelineEntry(
    string Title,
    DateTime DateTime,
    string? Description = null,
    TimelineEntryDirection Direction = TimelineEntryDirection.Up);