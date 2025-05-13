using Olve.Paths;
using Spectre.Console.Cli;
using Path = Olve.Paths.Path;

namespace TaskDrawer.CLI;

public class RunCommand : Command<RunCommand.Settings>
{
    public enum RunMode
    {
        Default,
        Overwrite,
        Watch
    }
    
    public class Settings : CommandSettings
    {
        
        [CommandArgument(0, "[Input]")]
        public required string Input { get; set; }
        
        [CommandArgument(1, "[Output]")]
        public required string Output { get; set; }
        
        [CommandOption("-M|--mermaid")]
        public string? Mermaid { get; set; }
        
        [CommandOption("-m|--mode")]
        public RunMode Mode { get; set; } = RunMode.Default;
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        var input = Path.Create(settings.Input);
        var output = Path.Create(settings.Output);
        var mermaid = settings.Mermaid != null ? Path.Create(settings.Mermaid) : null;

        return settings.Mode switch
        {
            RunMode.Default => ExecuteSingle(input, output, mermaid),
            RunMode.Overwrite => ExecuteSingle(input, output, mermaid, true),
            RunMode.Watch => ExecuteWatch(input, output, mermaid),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static int ExecuteSingle(IPath input, IPath output, IPath? mermaid, bool overwrite = false)
    {
        TaskFileToGraphImageOperation.Request request = new(input, output, mermaid, overwrite);
        
        var operation = CommandHelper.GetOperation<TaskFileToGraphImageOperation>();
        return CommandHelper.ExecuteOperation(operation, request);
    }

    private static int ExecuteWatch(IPath input, IPath output, IPath? mermaid)
    {
        WatchTaskFileToGraphImageOperation.Request request = new(input, output, mermaid);
        
        var operation = CommandHelper.GetOperation<WatchTaskFileToGraphImageOperation>();
        return CommandHelper.ExecuteOperation(operation, request);
    }
    
}