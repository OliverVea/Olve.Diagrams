using Olve.Diagrams.Flowchart;
using Olve.Results.TUnit;

namespace Olve.Diagrams.Tests.Flowchart;

public class BlockedStateTests
{
    [Test]
    public async System.Threading.Tasks.Task ExplicitBlocked_PropagatesToDependents()
    {
        string[] lines =
        {
            "1. initial",
            "  a. (blocked) first step",
            "  b. second step [1a]",
            "2. final step [1]"
        };

        var result = TaskListParser.ParseTasks(lines);
        await Assert.That(result).Succeeded();
        var tasks = result.Value!;

        var task1 = tasks[0];
        var stepA = task1.SubTasks[0];
        var stepB = task1.SubTasks[1];
        var final = tasks[1];

        await Assert.That(stepA.Blocked).IsTrue();
        await Assert.That(stepB.Blocked).IsTrue();
        await Assert.That(final.Blocked).IsTrue();
    }
}
