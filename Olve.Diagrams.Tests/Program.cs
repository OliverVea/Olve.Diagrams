using Olve.Diagrams.Flowchart;
using Olve.Results;

namespace Olve.Diagrams.Tests;

public class Program
{
    private const string Data = """
                        1. investigate if it's possible
                          a. (done) read the code
                          b. (done) try through the api [1a]
                        2. (done) report the findings
                          a. write report [1]
                          b. make powerpoint [1]
                        3. present for the ceo [2]
                        """;
    
    [Test]
    public System.Threading.Tasks.Task Test()
    {
        var result = Run();
        if (result.TryPickProblems(out var problems))
        {
            foreach (var problem in problems)
            {
                Console.WriteLine(problem.ToDebugString());
            }
            
            Assert.Fail("Test failed with problems.");
        }
        
        return System.Threading.Tasks.Task.CompletedTask;
    }

    private static Result Run()
    {
        var lines = Data.Split('\n');
        var mermaidResult = Result.Chain(
            () => TaskListParser.ParseTasks(lines),
            MermaidGenerator.GenerateMermaidSource);
        
        if (mermaidResult.TryPickProblems(out var problems, out var mermaidSource))
        {
            return problems;
        }
        
        Console.WriteLine(mermaidSource.Source);

        return Result.Success();
    }
}