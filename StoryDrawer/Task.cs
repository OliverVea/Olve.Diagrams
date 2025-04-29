using System.Text;

namespace TaskDrawer;

public class Task(TaskName name, string description)
{
    public TaskName Name { get; } = name;
    public string Description { get; } = description;
    public bool Done { get; set; }
    public Task? Parent { get; set; }
    public List<Task> SubTasks { get; set; } = [];
    public Task[] Blockers { get; set; } = [];
    
    public bool Blocked => Blockers.Any(x => !x.Done) || SubTasks.Any(x => !x.Done);
    
    public override string ToString()
    {
        StringBuilder sb = new();

        if (Done)
        {
            sb.Append("(done) ");
        }
        
        sb.Append($"{Name.Value}: {Description}");
        
        if (Parent != null)
        {
            sb.Append($" Parent: {Parent.Name.Value}");
        }
        
        return sb.ToString();
    }
}