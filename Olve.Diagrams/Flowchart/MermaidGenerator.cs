using Olve.Results;

namespace Olve.Diagrams.Flowchart;

public static class MermaidGenerator
{
    
    private const string ColorDone = "#272";
    private const string ColorNotDone = "#b91";
    private const string ColorBlocked = "#b00";
    
    private const string MermaidTemplate = """
                                           graph BT
                                               {{~ for task in Tasks ~}}
                                               {{ task.name }}["**{{ task.name }}:** {{ task.description }}"]
                                               {{~ end ~}}
                                           
                                               {{~ for task in Tasks ~}}
                                               {{~ for sub in task.subTasks ~}}
                                               {{ task.name }} --> {{ sub }}
                                               {{~ end ~}}
                                               {{~ for blocker in task.blockers ~}}
                                               {{ task.name }} --> {{ blocker }}
                                               {{~ end ~}}
                                               {{~ end ~}}
                                               
                                               {{~ for task in Tasks ~}}
                                               style {{ task.name }} fill:{{ if task.done }}{{ ColorDone }}{{ else if task.blocked }}{{ ColorBlocked }}{{ else }}{{ ColorNotDone }}{{ end }}
                                               {{~ end ~}}
                                           """;

    public static Result<MermaidSource> GenerateMermaidSource(IReadOnlyList<Task> tasks)
    {
        var template = Scriban.Template.Parse(MermaidTemplate);

        if (template.HasErrors)
        {
            return new ResultProblem("Failed to parse Mermaid template: {0}", string.Join("; ", template.Messages));
        }

        var scribanTasks = tasks.Select(task => new
        {
            name = task.Name.Value,
            description = task.Description.Replace("\"", "\\\""),
            done = task.Done,
            blocked = task.Blocked,
            parent = task.Parent?.Name.Value,
            subTasks = task.SubTasks.Select(s => s.Name.Value).ToList(),
            blockers = task.Blockers.Select(b => b.Name.Value).ToList()
        }).ToList();

        var context = new { Tasks = scribanTasks, ColorDone, ColorNotDone, ColorBlocked };
        var rendered = template.Render(context, member => member.Name);

        return new MermaidSource(rendered);
    }
}