using System.Diagnostics;
using Olve.Paths;
using Olve.Results;

namespace TaskDrawer;

public static class MermaidRenderer
{
    public static Result RenderMermaid(IPath mermaidSourceInputPath, IPath mermaidImageOutputPath, bool overwrite = false)
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

        try
        {
            var psi = new ProcessStartInfo("/usr/bin/env")
            {
                Arguments             = $"mmdc --input \"{mermaidSourceInputPath.Path}\" --output \"{mermaidImageOutputPath.Path}\" -t dark -b transparent",
                UseShellExecute       = false,
                RedirectStandardError = true,
                RedirectStandardOutput= true,
            };
            
            psi.Environment["PATH"] =
                psi.Environment["PATH"] +
                ":/home/oliver/.nvm/versions/node/v23.8.0/bin";

            
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
                    return new ResultProblem("Mermaid CLI not found. Please install it with 'npm install -g @mermaid-js/mermaid-cli'");
                }
                
                return new ResultProblem("Failed to render mermaid image. Process exited with code: {0}", process?.ExitCode);
            }
            
            if (!mermaidImageOutputPath.Exists())
            {
                return new ResultProblem("Mermaid image output path does not exist after rendering: '{0}'", mermaidImageOutputPath);
            }
            
            return Result.Success();
        }
        catch (Exception e)
        {
            return new ResultProblem(e, "Failed to render mermaid image");
        }
    }
}