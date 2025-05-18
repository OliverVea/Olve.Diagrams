using Microsoft.Extensions.DependencyInjection;
using Olve.Operations;
using Spectre.Console;

namespace TaskDrawer.CLI;

public static class CommandHelper
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    public static T GetOperation<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public static int ExecuteOperation<TOperation, TRequest>(TOperation operation, TRequest request)
        where TOperation : IOperation<TRequest>
    {
        var result = operation.Execute(request);
        if (result.TryPickProblems(out var problems))
        {
            foreach (var problem in problems)
            {
                AnsiConsole.WriteLine(problem.ToDebugString());
            }

            return -1;
        }

        return 0;
    }

    public static async Task<int> ExecuteOperationAsync<TOperation, TRequest>(TOperation operation, TRequest request,
        CancellationToken ct = default)
        where TOperation : IAsyncOperation<TRequest>
    {
        var result = await operation.ExecuteAsync(request, ct);
        if (result.TryPickProblems(out var problems))
        {
            foreach (var problem in problems)
            {
                AnsiConsole.WriteLine(problem.ToDebugString());
            }

            return -1;
        }

        return 0;
    }
}