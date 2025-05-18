using Olve.Operations;
using Olve.Paths;
using Olve.Results;
using Path = Olve.Paths.Path;

namespace TaskDrawer.CLI;

public class TaskFileToGraphImageOperation(RenderMermaidOperation renderMermaidOperation) : IAsyncOperation<TaskFileToGraphImageOperation.Request>
{
    public record Request(IPath InputPath, IPath OutputPath, IPath? MermaidPath = null, bool Overwrite = false);

    public async Task<Result> ExecuteAsync(Request request, CancellationToken ct = default)
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
        
        await File.WriteAllTextAsync(mermaidPath.Path, mermaidSource.Source, ct);
        
        Console.WriteLine("Mermaid source written to: {0}", mermaidPath.GetLinkString());

        RenderMermaidOperation.Request renderMermaidRequest = new(mermaidPath, request.OutputPath, request.Overwrite);
        var result = await renderMermaidOperation.ExecuteAsync(renderMermaidRequest, ct);

        return result.IfProblem(p => p.Append(new ResultProblem("could not turn graph file into diagram")));
    }
    
    private IPath GetTempPath()
    {
        var tempDir = Directory.CreateTempSubdirectory();
        var tempDirPath = Path.Create(tempDir.FullName);
        
        var fileName = Guid.NewGuid() + ".txt";

        return tempDirPath / fileName;
    }
}