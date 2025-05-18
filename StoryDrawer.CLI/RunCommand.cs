using Olve.Paths;
using Spectre.Console.Cli;
using Path = Olve.Paths.Path;

namespace TaskDrawer.CLI;

public class RunCommand : AsyncCommand<RunCommand.Settings>
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

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var input = Path.Create(settings.Input);
        var output = Path.Create(settings.Output);
        var mermaid = settings.Mermaid != null ? Path.Create(settings.Mermaid) : null;

        return settings.Mode switch
        {
            RunMode.Default => ExecuteSingleAsync(input, output, mermaid),
            RunMode.Overwrite => ExecuteSingleAsync(input, output, mermaid, true),
            RunMode.Watch => ExecuteWatch(input, output, mermaid),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static Task<int> ExecuteSingleAsync(IPath input, IPath output, IPath? mermaid, bool overwrite = false, CancellationToken ct = default)
    {
        TaskFileToGraphImageOperation.Request request = new(input, output, mermaid, overwrite);
        
        var operation = CommandHelper.GetOperation<TaskFileToGraphImageOperation>();
        return CommandHelper.ExecuteOperationAsync(operation, request, ct);
    }

    private static Task<int> ExecuteWatch(IPath input, IPath output, IPath? mermaid, CancellationToken ct = default)
    {
        WatchTaskFileToGraphImageOperation.Request request = new(input, output, mermaid);
        
        var operation = CommandHelper.GetOperation<WatchTaskFileToGraphImageOperation>();
        return CommandHelper.ExecuteOperationAsync(operation, request, ct);
    }
    
}