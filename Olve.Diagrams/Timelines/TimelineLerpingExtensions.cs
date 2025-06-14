using Olve.Results;

namespace Olve.Diagrams.Timelines;

public static class TimelineLerpingExtensions
{
    public static Result<DateTime> Lerp(this Timeline timeline, double t, bool allowExtrapolation = false)
    {
        if (ValidateExtrapolation(t, allowExtrapolation).TryPickProblems(out var problems))
        {
            return problems.Prepend("Lerp value is out of bounds.");
        }
        
        if (timeline.GetValidatedStartEnd().TryPickProblems(out problems, out var times))
        {
            return problems.Prepend("Cannot lerp timeline.");
        }
        
        var (start, end) = times;
        var tDelta = end.Ticks - start.Ticks;
        var tLerped = start.Ticks + (long)(t * tDelta);
        return new DateTime(tLerped);
    }
    
    public static Result<double> InverseLerp(this Timeline timeline, DateTime time, bool allowExtrapolation = false)
    {
        if (timeline.GetValidatedStartEnd().TryPickProblems(out var problems, out var times))
        {
            return problems.Prepend("Cannot inverse lerp timeline.");
        }
        
        var (start, end) = times;
        var tDelta = end.Ticks - start.Ticks;
        if (tDelta == 0)
        {
            return new ResultProblem("Cannot inverse lerp timeline: start and end times are the same.");
        }
        
        var t = (time.Ticks - start.Ticks) / (double)tDelta;
        
        if (ValidateExtrapolation(t, allowExtrapolation).TryPickProblems(out problems))
        {
            return problems.Prepend("Inverse lerp value is out of bounds.");
        }
        
        return t;
    }
    
    private static Result<(DateTime Start, DateTime End)> GetValidatedStartEnd(this Timeline timeline)
    {
        if (timeline.Start.TryPickProblems(out var startProblems, out var start))
        {
            return startProblems.Prepend("Start time is not available.");
        }

        if (timeline.End.TryPickProblems(out var endProblems, out var end))
        {
            return endProblems.Prepend("End time is not available.");
        }

        if (start == end)
        {
            return new ResultProblem("Start and end times are the same.");
        }

        return (start, end);
    }
    
    private static Result ValidateExtrapolation(double t, bool allowExtrapolation)
    {
        if (!allowExtrapolation && t is < 0 or > 1)
        {
            return new ResultProblem("t must be between 0 and 1, inclusive.");
        }
        
        return Result.Success();
    }
}