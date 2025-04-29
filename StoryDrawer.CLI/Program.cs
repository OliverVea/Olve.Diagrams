// See https://aka.ms/new-console-template for more information

using CommandDotNet;
using Microsoft.Extensions.DependencyInjection;
using Olve.Operations;
using Olve.Paths;
using Olve.Results;
using Path = Olve.Paths.Path;

namespace TaskDrawer.CLI;

public class Program
{
    public static ServiceProvider ServiceProvider = null!;
    
    public static int Main(string[] args)
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddTransient<TaskFileToGraphImageOperation>();
        serviceCollection.AddTransient<WatchTaskFileToGraphImageOperation>();

        ServiceProvider = serviceCollection.BuildServiceProvider();

        return new AppRunner<Commands>().Run(args);
    }
}


public class Commands
{
    public int ParseFile(string inputFile, string outputFile, string? mermaidFile = null, bool overwrite = false)
    {
        var inputPath = Path.Create(inputFile);
        var outputPath = Path.Create(outputFile);
        var mermaidPath = mermaidFile != null ? Path.Create(mermaidFile) : null;
        
        TaskFileToGraphImageOperation.Request request = new(inputPath, outputPath, mermaidPath, overwrite);
        
        var operation = GetOperation<TaskFileToGraphImageOperation>();
        return ExecuteOperation(operation, request);
    }
    
    public int WatchFile(string inputFile, string outputFile, string? mermaidFile = null)
    {
        var inputPath = Path.Create(inputFile);
        var outputPath = Path.Create(outputFile);
        var mermaidPath = mermaidFile != null ? Path.Create(mermaidFile) : null;
        
        WatchTaskFileToGraphImageOperation.Request request = new(inputPath, outputPath, mermaidPath);
        
        var operation = GetOperation<WatchTaskFileToGraphImageOperation>();
        return ExecuteOperation(operation, request);
    }
    
    private static T GetOperation<T>() where T : notnull
    {
        return Program.ServiceProvider.GetRequiredService<T>();
    }

    private static int ExecuteOperation<TOperation, TRequest>(TOperation operation, TRequest request)
        where TOperation : notnull, IOperation<TRequest>
    {
        var result = operation.Execute(request);
        if (result.TryPickProblems(out var problems))
        {
            foreach (var problem in problems)
            {
                Console.WriteLine(problem.ToDebugString());
            }

            return -1;
        }

        return 0;
    }
}

public class WatchTaskFileToGraphImageOperation(TaskFileToGraphImageOperation taskFileToGraphImageOperation) : IOperation<WatchTaskFileToGraphImageOperation.Request>
{
    public record Request(IPath InputPath, IPath OutputPath, IPath? MermaidPath = null, TimeSpan? Interval = null);

    private bool _changed = true;
    
    public Result Execute(Request request)
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
    
    private void WorkerMethod(Request request)
    {
        var interval = request.Interval ?? TimeSpan.FromMilliseconds(50);
        TaskFileToGraphImageOperation.Request refreshRequest =
            new(request.InputPath, request.OutputPath, request.MermaidPath, true);
        
        while (true)
        {
            if (_changed)
            {
                _changed = false;
                
                var result = taskFileToGraphImageOperation.Execute(refreshRequest);
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

public class TaskFileToGraphImageOperation : IOperation<TaskFileToGraphImageOperation.Request>
{
    public record Request(IPath InputPath, IPath OutputPath, IPath? MermaidPath = null, bool Overwrite = false);

    public Result Execute(Request request)
    {
        if (request.InputPath.Exists() == false)
        {
            return new ResultProblem("Input file does not exist: '{0}'", request.InputPath);
        }
        
        if (request.OutputPath.Exists() && request.Overwrite == false)
        {
            return new ResultProblem("Output file already exists: '{0}'. Use --overwrite to overwrite.", request.OutputPath);
        }

        string[] taskList;
        
        try
        {
            taskList = File.ReadLines(request.InputPath.Path).ToArray();
        }
        catch (Exception e)
        {
            return new ResultProblem(e, "Could not read input file");
        }

        var taskListResult = TaskListParser.ParseTasks(taskList);
        if (taskListResult.TryPickProblems(out var problems, out var tasks))
        {
            return problems;
        }

        var mermaidResult = MermaidGenerator.GenerateMermaidSource(tasks);
        if (mermaidResult.TryPickProblems(out problems, out var mermaidSource))
        {
            return problems;
        }
        
        var mermaidPath = request.MermaidPath ?? GetTempPath();
        
        File.WriteAllText(mermaidPath.Path, mermaidSource.Source);
        
        Console.WriteLine("Mermaid source written to: {0}", mermaidPath.GetLinkString());

        return MermaidRenderer.RenderMermaid(mermaidPath, request.OutputPath, request.Overwrite);
    }
    
    private IPath GetTempPath()
    {
        var tempDir = Directory.CreateTempSubdirectory();
        var tempDirPath = Path.Create(tempDir.FullName);
        
        var fileName = Guid.NewGuid() + ".txt";

        return tempDirPath / fileName;
    }
}