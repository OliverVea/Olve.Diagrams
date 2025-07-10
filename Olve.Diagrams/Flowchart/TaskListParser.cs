using Olve.Results;

namespace Olve.Diagrams.Flowchart;

public static class TaskListParser
{
    // 1. do some work
    //   a. (done) do half of the work
    //   b. do the other half [1a]
    // 2. report on the work [1]
    
    // Notes: a task's name should be prefixed with the parent tasks names. For example a under 1 is "1a". 
    public static Result<IReadOnlyList<Task>> ParseTasks(IReadOnlyList<string> taskList)
    {
        if (taskList.Count == 0)
        {
            return new List<Task>();
        }

        var tasks = new List<Task>();
        Stack<Task> parentStack = new();

        foreach (var taskLine in taskList)
        {
            var result = ParseTaskLine(taskLine, parentStack);
            if (result.TryPickProblems(out var problems, out var task))
            {
                return problems;
            }
            
            tasks.Add(task);
            if (parentStack.TryPeek(out var parent))
            {
                parent.SubTasks.Add(task);
            }
            
            parentStack.Push(task);
        }

        Dictionary<TaskName, Task> taskLookup = new();

        void AddToLookup(Task task)
        {
            taskLookup[task.Name] = task;
            taskLookup[new TaskName(task.QualifiedName)] = task;

            foreach (var subTask in task.SubTasks)
            {
                AddToLookup(subTask);
            }
        }

        foreach (var task in tasks)
        {
            AddToLookup(task);
        }

        foreach (var task in taskLookup.Values)
        {
            task.Blockers = task.Blockers
                .Where(x => taskLookup.ContainsKey(x.Name))
                .Select(x => taskLookup[x.Name])
                .ToArray();
        }
 
        return tasks;
    }
    
    private static Result<Task> ParseTaskLine(string taskLine, Stack<Task> parentStack)
    {
        var result = LineRegex.Apply(taskLine);
        if (result.TryPickProblems(out var problems, out var match))
        {
            return problems;
        }
        
        return ToTask(match, parentStack);
    }

    private const int IndentationWidth = 2;
    private static Result<Task> ToTask(LineRegexMatch regexMatch, Stack<Task> parentStack)
    {
        var id = regexMatch.Id;
        var description = regexMatch.Description;
        var blockers = ParseBlockers(regexMatch.Blockers);
        var level = regexMatch.Indentation.Length / IndentationWidth;
        var done = regexMatch.Done;
        var blockedExplicit = regexMatch.Blocked;
        
        while (parentStack.Count > level)
        {
            if (!parentStack.TryPop(out _))
            {
                return new ResultProblem("Error with indentation of task '{0}'", regexMatch.Input);
            }
        }

        parentStack.TryPeek(out var parent);

        return new Task(new TaskName(id), description)
        {
            Done = done,
            ExplicitBlocked = blockedExplicit,
            Parent = parent,
            Blockers = blockers
        };
    }

    private static Task[] ParseBlockers(string regexMatchBlockers)
    {
        var blockers = regexMatchBlockers.Split([' ', '[', ']'], StringSplitOptions.RemoveEmptyEntries);
        var blockingTasks = new List<Task>();
        
        foreach (var blocker in blockers)
        {
            if (string.IsNullOrWhiteSpace(blocker))
            {
                continue;
            }

            var temporaryTask = new Task(new TaskName(blocker), string.Empty);
            blockingTasks.Add(temporaryTask);
        }

        return blockingTasks.ToArray();
    }
}