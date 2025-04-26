using Olve.Utilities.Ids;

namespace TaskDrawer;

public class Story(Id<Story> id, StoryName name, string description, List<Issue> issues)
{
    public Id<Story> Id { get; init; } = id;
    public StoryName Name { get; init; } = name;
    public string Description { get; init; } = description;
    public List<Issue> Issues { get; init; } = issues;
}