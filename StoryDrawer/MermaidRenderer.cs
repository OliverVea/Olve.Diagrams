using System.Diagnostics;
using Olve.Operations;
using Olve.Paths;
using Olve.Results;
using TaskDrawer.Docker;

namespace TaskDrawer;

public class RenderMermaidOperation(DockerRunOperation dockerRunOperation) : IAsyncOperation<RenderMermaidOperation.Request>
{
    private const string Theme = "dark";
    private const string BackgroundColor = "transparent";
    
    private static readonly DockerImage MermaidCliImage = new("ghcr.io/mermaid-js/mermaid-cli/mermaid-cli");
    private static readonly IPurePath DockerSourcePath = Olve.Paths.Path.CreatePure("/data/input.mmd");
    private static readonly IPurePath DockerOutputPath = Olve.Paths.Path.CreatePure("/data/output.svg");

    public record Request(IPath SourcePath, IPath OutputPath, bool Overwrite = false);
    
    public async Task<Result> ExecuteAsync(Request request, CancellationToken ct = default)
    {
        if (!request.SourcePath.Exists())
        {
            return new ResultProblem("Mermaid source input path does not exist: '{0}'", request.SourcePath);
        }

        if (request.OutputPath.Exists())
        {
            if (request.Overwrite)
            {
                //File.Delete(mermaidImageOutputPath.Path);
            }
            else
            {
                return new ResultProblem("Mermaid image output path already exists: '{0}'", request.OutputPath);
            }
        }
        else
        {
            Directory.CreateDirectory(request.OutputPath.Parent.Path);
            await File.Create(request.OutputPath.Path).DisposeAsync();
            
#if !WINDOWS
            // on Linux: make it 0666 so anyone (including container users) can open it
            var psi = new ProcessStartInfo("chmod", $"666 \"{request.OutputPath.Path}\"")
            {
                UseShellExecute = false
            };

            await (Process.Start(psi)?.WaitForExitAsync(ct) ?? System.Threading.Tasks.Task.CompletedTask);
#endif
        }

        DockerRunOperation.Request dockerRunRequest = new(MermaidCliImage)
        {
            Name = "mermaid",
            Arguments =
            [
                "-i " + DockerSourcePath.Name,
                "-o " + DockerOutputPath.Name,
                "-t " + Theme,
                "-b " + BackgroundColor
            ],
            PathBindings =
            [
                new PathBinding(request.SourcePath, DockerSourcePath),
                new PathBinding(request.OutputPath, DockerOutputPath)
            ]
        };

        var result = await dockerRunOperation.ExecuteAsync(dockerRunRequest, ct);

        return result;
    }
}