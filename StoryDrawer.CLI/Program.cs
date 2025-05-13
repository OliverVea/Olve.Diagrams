// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace TaskDrawer.CLI;

public class Program
{

    public static int Main(string[] args)
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddTransient<TaskFileToGraphImageOperation>();
        serviceCollection.AddTransient<WatchTaskFileToGraphImageOperation>();

        CommandHelper.SetServiceProvider(serviceCollection.BuildServiceProvider());

        var app = new CommandApp();

        app.SetDefaultCommand<RunCommand>();

        return app.Run(args);
    }
}