// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using TaskDrawer.Docker;

namespace TaskDrawer.CLI;

public class Program
{

    public static int Main(string[] args)
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddTransient<TaskFileToGraphImageOperation>();
        serviceCollection.AddTransient<WatchTaskFileToGraphImageOperation>();
        serviceCollection.AddTransient<RenderMermaidOperation>();
        serviceCollection.AddTransient<DockerRunOperation>();

        CommandHelper.SetServiceProvider(serviceCollection.BuildServiceProvider());

        var app = new CommandApp();

        app.SetDefaultCommand<RunCommand>();

        return app.Run(args);
    }
}