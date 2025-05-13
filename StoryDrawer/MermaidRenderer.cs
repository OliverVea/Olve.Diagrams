using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using Olve.Paths;
using Olve.Results;
using Olve.Utilities.Assertions;

namespace TaskDrawer;

public static class MermaidRenderer
{
    private const string Theme = "dark";
    private const string BackgroundColor = "transparent";
    
    public static async Task<Result> RenderMermaidAsync(IPath mermaidSourceInputPath, IPath mermaidImageOutputPath, bool overwrite = false)
    {
        if (!mermaidSourceInputPath.Exists())
        {
            return new ResultProblem("Mermaid source input path does not exist: '{0}'", mermaidSourceInputPath);
        }

        if (mermaidImageOutputPath.Exists())
        {
            if (overwrite)
            {
                //File.Delete(mermaidImageOutputPath.Path);
            }
            else
            {
                return new ResultProblem("Mermaid image output path already exists: '{0}'", mermaidImageOutputPath);
            }
        }
        

        var workingDir = mermaidSourceInputPath.Parent;
        
        Console.WriteLine($"Full file path: {mermaidSourceInputPath.Path}");
        Console.WriteLine($"Working dir: {workingDir.Path}");
        Console.WriteLine($"Expecting: {(workingDir / (mermaidSourceInputPath.Name ?? string.Empty)).Path}");
        
        var dockerArgs = $"run --rm -v \"{mermaidSourceInputPath.Path}:/data/input.mmd\" ghcr.io/mermaid-js/mermaid-cli/mermaid-cli " +
                         $"-i \"diagram.mmd\" -o \"/data/{mermaidImageOutputPath.Name}\" " +
                         $"-t {Theme} -b {BackgroundColor}";

        Console.WriteLine("docker " + dockerArgs);
        
        try
        {
            var psi = new ProcessStartInfo("docker")
            {
                Arguments = dockerArgs,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = Process.Start(psi);
            process?.WaitForExit();

            if (process is not { ExitCode: 0 })
            {
                var errorOutput = process?.StandardError.ReadToEnd();
                var output = process?.StandardOutput.ReadToEnd();
                var errorMessage = string.IsNullOrWhiteSpace(errorOutput) ? output : errorOutput;

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return new ResultProblem("Failed to render mermaid image. Error: {0}", errorMessage);
                }

                if (process?.ExitCode == 127)
                {
                    return new ResultProblem(
                        "Mermaid CLI not found. Please install it with 'npm install -g @mermaid-js/mermaid-cli'");
                }

                var exitCode = process?.ExitCode.ToString() ?? "null";
                return new ResultProblem("Failed to render mermaid image. Process exited with code: {0}", exitCode);
            }

            Assert.That(mermaidImageOutputPath.Exists, "Mermaid image output path does not exist after rendering: '{0}'");

            return Result.Success();
        }
        catch (Exception e)
        {
            return new ResultProblem(e, "Failed to render mermaid image");
        }
    }
}