namespace Olve.Diagrams.Timelines;

internal class TimelineSerializationConstants
{
    public const string TimelineFormatVersion = "1.0";
    
    public const string DateFormat = "yyyy-MM-dd";
    public const char HeaderSeparator = ':';
    public const string SectionSeparator = "---";
    
    public const string VersionHeaderKey = "version";
    public const string TitleHeaderKey = "title";
    public const string StartHeaderKey = "start";
    public const string EndHeaderKey = "end";

    public const char DirectionUpKey = '^';
    public const char DirectionDownKey = 'v';
}