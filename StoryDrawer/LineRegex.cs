using System.Text.RegularExpressions;
using Olve.Results;

namespace TaskDrawer;

public static partial class LineRegex
{
    public static Result<LineRegexMatch> Apply(string input)
    {
        var regex = MyRegex();
        var match = regex.Match(input);
        
        if (!match.Success)
        {
            return new ResultProblem("Failed to match line: '{0}'", input);
        }

        return new LineRegexMatch(
            input,
            match.Groups["indent"].Value,
            match.Groups["id"].Value[0..^1],
            match.Groups["done"].Success,
            match.Groups["desc"].Value,
            match.Groups["blockers"].Value);
    }
    
    [GeneratedRegex(@"(?<indent>\s*)(?<id>(?:\d+\.)|(?:[a-z]\.))\s*(?:(?<done>\(done\))\s*)?(?<desc>.*?)\s*(?=\s*\[|$)\s*(?<blockers>(?:\s*\[[^\]]+\])*)\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    public static partial Regex MyRegex();
}