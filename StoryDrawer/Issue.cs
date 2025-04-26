using Olve.Utilities.Ids;

namespace TaskDrawer;

public class Issue(Id<Issue> id, IssueName name, Story parent, string description)
{
    public Id<Issue> Id { get; init; } = id;
    public IssueName Name { get; set; } = name;
    public Story Parent { get; init; } = parent;
    public string Description { get; init; } = description;
}