using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SIGRA.Data;
using SIGRA.Data.Models;

namespace SIGRA.Services.Providers;

public interface IHolidayProvider
{
    Task<HashSet<DateOnly>> GetHolidaysAsync(int year);
}

public class HolidayProvider : IHolidayProvider
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public HolidayProvider(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<HashSet<DateOnly>> GetHolidaysAsync(int year)
    {
        var cacheKey = $"holidays_{year}";

        if (_cache.TryGetValue(cacheKey, out HashSet<DateOnly>? cached))
            return cached!;

        var holidays = await _db.JoursFeries
            .Where(h => h.Date.Year == year)
            .Select(h => h.Date)
            .ToListAsync();

        var result = holidays.ToHashSet();

        // Cache 24h — les jours fériés ne changent pas en cours de journée
        _cache.Set(cacheKey, result, TimeSpan.FromHours(24));

        return result;
    }
}
