    using System.Diagnostics;
    using Olve.Operations;
    using Olve.Paths;
    using Olve.Results;

    namespace TaskDrawer.Docker;

    public readonly record struct DockerImage(string Value);
    public readonly record struct PathBinding(IPath HostPath, IPurePath ContainerPath);

    public class DockerRunOperation : IAsyncOperation<DockerRunOperation.Request>
    {
        public class Request(DockerImage image)
        {
            public DockerImage Image { get; } = image;
            public string? Name { get; init; }
            public PathBinding[]? PathBindings { get; init; }
            public string[]? Arguments { get; init; }
            public TimeSpan? Timeout { get; init; }
            
        }

        public async Task<Result> ExecuteAsync(Request request, CancellationToken ct = default)
        {
            var arguments = GetProcessArguments(request);

            var psi = new ProcessStartInfo("docker")
            {
                Arguments = arguments,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true
            };

            Console.WriteLine("docker " + arguments);

        var process = Process.Start(psi);
            if (process is null)
            {
                return new ResultProblem("Could not start Docker process.");
            }

            if (request.Timeout.HasValue)
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(request.Timeout.Value);
                ct = cts.Token;
            }

            try
            {
                await process.WaitForExitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(); } catch { /* best effort */ }
                return new ResultProblem($"Docker run was cancelled or timed out after {request.Timeout}.");
            }

            if (ExitedSuccessfully(process))
            {
                return Result.Success();
            }

            return new ResultProblem($"Docker exited with code {process.ExitCode}.");
        }

        private static string GetProcessArguments(Request request)
        {
            List<string> parts = [
                "run",
                "--rm"
            ];

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                parts.Add("--name");
                parts.Add(request.Name!);
            }

            // mounts: -v host:container
            if (request.PathBindings is not null)
            {
                foreach (var binding in request.PathBindings)
                {
                    // wrap in quotes in case paths contain spaces
                    var host = binding.HostPath.Path.Replace("\"", "\\\"");
                    var container = binding.ContainerPath.Path.Replace("\"", "\\\"");
                    parts.Add("-v");
                    parts.Add($"\"{host}:{container}\"");
                }
            }

            // the image to run
            parts.Add(request.Image.Value);

            // any extra args to the container
            if (request.Arguments is not null && request.Arguments.Length > 0)
            {
                foreach (var arg in request.Arguments)
                {
                    parts.Add(arg);
                }
            }

            // join using spaces
            return string.Join(' ', parts);
        }

        private static bool ExitedSuccessfully(Process process) => process is { HasExited: true, ExitCode: 0 };
    }

    /*

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
    */