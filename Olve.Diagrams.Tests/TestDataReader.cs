using Olve.Paths;
using Olve.Paths.Glob;
using Olve.Results;
using Path = Olve.Paths.Path;

namespace Olve.Diagrams.Tests;

public static class TestDataReader
{
    public readonly record struct TestDataFile(string FileName, string Content);

    public static Result<IReadOnlyList<TestDataFile>> GetTestDataFiles(string directoryName, string pattern = "*")
    {
        if (GetTestDirectory(directoryName).TryPickProblems(out var problems, out var testDirectory))
        {
            return problems.Prepend("Failed to locate test directory '{0}'", directoryName);
        }

        if (!testDirectory.TryGlob(pattern, out var files))
        {
            return new ResultProblem("Failed to glob files in directory '{0}' with pattern '{1}'", testDirectory, pattern);
        }

        var fileList = files.ToArray();
        var testDataFiles = new TestDataFile[fileList.Length];

        for (var i = 0; i < fileList.Length; i++)
        {
            var file = fileList[i];

            if (!file.TryGetAbsolute(out var absolutePath))
            {
                return new ResultProblem("Could not resolve absolute path for file '{0}'", file.Path);
            }

            if (file.Name is not { } fileName)
            {
                return new ResultProblem("File name was null or invalid for path '{0}'", file.Path);
            }

            var fileContent = File.ReadAllText(absolutePath.Path).Trim();
            testDataFiles[i] = new TestDataFile(fileName, fileContent);
        }

        return testDataFiles;
    }

    private static Result<IPath> GetTestDirectory(string directoryName)
    {
        if (!Path.TryGetAssemblyExecutable(out var assemblyPath))
        {
            return new ResultProblem("Failed to get assembly executable path");
        }

        if (!assemblyPath.TryGetParent(out var assemblyDirectory))
        {
            return new ResultProblem("Failed to get parent of assembly path '{0}'", assemblyPath.Path);
        }

        var testDirectory = assemblyDirectory / "testdata" / directoryName;
        if (!testDirectory.Exists())
        {
            return new ResultProblem("Test data directory does not exist: '{0}' for input directory '{1}'", testDirectory, directoryName);
        }

        return Result<IPath>.Success(testDirectory);
    }
}
