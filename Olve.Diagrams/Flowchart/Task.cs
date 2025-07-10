using System.Text;

namespace Olve.Diagrams.Flowchart;

public class Task(TaskName name, string description)
{
    public TaskName Name { get; } = name;
    public string QualifiedName => string.Join("", Parents.Select(x => x.Name.Value));
    public string Description { get; } = description;
    public bool Done { get; set; }
    public bool ExplicitBlocked { get; set; }
    public Task? Parent { get; set; }
    public List<Task> SubTasks { get; set; } = [];
    public Task[] Blockers { get; set; } = [];

    public bool Blocked => ExplicitBlocked ||
        Blockers.Any(x => x.Blocked || !x.Done) ||
        SubTasks.Any(x => x.Blocked || !x.Done);

    public IEnumerable<Task> Parents => GetParents(this);

    private IEnumerable<Task> GetParents(Task task)
    {
        if (Parent is not { } parent) return [];
        
        return GetParents(parent).Append(parent);
    }
    
    public override string ToString()
    {
        StringBuilder sb = new();

        var parent = Parent;
        var levels = 0;
        while (parent != null)
        {
            levels++;
            parent = parent.Parent;
        }

        sb.Append(' ', levels * 2);

        sb.Append(Name.Value);
        sb.Append(". ");
        
        if (Done)
        {
            sb.Append("(done) ");
        }

        if (ExplicitBlocked)
        {
            sb.Append("(blocked) ");
        }
        
        sb.Append(Description);

        if (Blockers.Any()) sb.Append(' ');

        foreach (var blocker in Blockers)
        {
            sb.Append($"[{blocker.Name.Value}]");
        }
        
        return sb.ToString();
    }
}