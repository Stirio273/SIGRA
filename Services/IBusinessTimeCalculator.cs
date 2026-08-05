namespace SIGRA.Services;

public interface IBusinessTimeCalculator
{
    Task<DateTime> AddBusinessTimeAsync(DateTime start, TimeSpan duration);
    Task<TimeSpan> GetElapsedBusinessTimeAsync(DateTime start, DateTime end);
    Task<bool> IsBusinessDayAsync(DateTime date);
}
