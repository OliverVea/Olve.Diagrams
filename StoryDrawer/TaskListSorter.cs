namespace TaskDrawer;

public static class TaskListSorter
{
    public static IReadOnlyList<Task> GetSortedTasks(this IReadOnlyList<Task> tasks)
    {
        var sortedTasks = new List<Task>(tasks.Count);
        var visited = new HashSet<Task>();

        foreach (var task in tasks)
        {
            SortDfs(task, sortedTasks, visited);
        }

        return sortedTasks;
    }

    private static void SortDfs(Task task, List<Task> sorted, HashSet<Task> visited)
    {
        if (!visited.Add(task))
        {
            return;
        }

        foreach (var blocker in task.Blockers)
        {
            SortDfs(blocker, sorted, visited);
        }

        if (!sorted.Contains(task))
        {
            sorted.Add(task);
        }

        foreach (var subtask in task.SubTasks)
        {
            SortDfs(subtask, sorted, visited);
        }
    }
}