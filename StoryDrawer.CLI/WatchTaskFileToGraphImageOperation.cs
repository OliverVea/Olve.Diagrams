using Olve.Operations;
using Olve.Paths;
using Olve.Results;

namespace TaskDrawer.CLI;

public class WatchTaskFileToGraphImageOperation(TaskFileToGraphImageOperation taskFileToGraphImageOperation) : IAsyncOperation<WatchTaskFileToGraphImageOperation.Request>
{
    public record Request(IPath InputPath, IPath OutputPath, IPath? MermaidPath = null, TimeSpan? Interval = null);

    private bool _changed = true;
    
    public async Task<Result> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (request.InputPath.Name is not { } name)
        {
            return new ResultProblem("Input path '{0}' is invalid.", request.InputPath.Path);
        }
        
        FileSystemWatcher fileSystemWatcher = new(request.InputPath.Parent.Absolute.Path);
        fileSystemWatcher.EnableRaisingEvents = true;
        fileSystemWatcher.IncludeSubdirectories = false;
        fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
        fileSystemWatcher.Filter = name;
        
        fileSystemWatcher.Changed += (_, _) =>
        {
            _changed = true;
        };
        
        fileSystemWatcher.Created += (_, _) =>
        {
            _changed = true;
        };
        
        fileSystemWatcher.Deleted += (_, _) =>
        {
            _changed = true;
        };
        
        fileSystemWatcher.Renamed += (_, _) =>
        {
            _changed = true;
        };
        
        var cancellationTokenSource = new CancellationTokenSource();
        
        var workerTask = System.Threading.Tasks.Task.Run(() => WorkerMethod(request), cancellationTokenSource.Token);
        
        Console.WriteLine("Press any key to stop watching...");
        
        Console.ReadKey();
        
        fileSystemWatcher.Dispose();
        
        cancellationTokenSource.Cancel();
        workerTask.Wait(cancellationTokenSource.Token);
        
        Console.WriteLine("Stopped watching.");
        
        return Result.Success();
    }
    
    private async System.Threading.Tasks.Task WorkerMethod(Request request)
    {
        var interval = request.Interval ?? TimeSpan.FromMilliseconds(50);
        TaskFileToGraphImageOperation.Request refreshRequest =
            new(request.InputPath, request.OutputPath, request.MermaidPath, true);
        
        while (true)
        {
            if (_changed)
            {
                _changed = false;
                
                var result = await taskFileToGraphImageOperation.ExecuteAsync(refreshRequest);
                if (result.TryPickProblems(out var problems))
                {
                    foreach (var problem in problems)
                    {
                        Console.WriteLine(problem.ToDebugString());
                    }
                }
            }

            Thread.Sleep(interval);
        }
    }
}