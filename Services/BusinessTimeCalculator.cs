using Microsoft.Extensions.Options;
using SIGRA.Domain.Options;
using SIGRA.Services.Providers;

namespace SIGRA.Services;

public class BusinessTimeCalculator : IBusinessTimeCalculator
{
    private readonly BusinessHoursOptions _options;
    private readonly IHolidayProvider _holidayProvider;

    public BusinessTimeCalculator(
        IOptions<BusinessHoursOptions> options,
        IHolidayProvider holidayProvider)
    {
        _options = options.Value;
        _holidayProvider = holidayProvider;
    }

    public async Task<bool> IsBusinessDayAsync(DateTime date)
    {
        if (!_options.WorkingDays.Contains(date.DayOfWeek))
            return false;

        var holidays = await _holidayProvider.GetHolidaysAsync(date.Year);
        return !holidays.Contains(DateOnly.FromDateTime(date.Date));
    }

    // Ajoute une durée OUVRÉE à une date de départ
    public async Task<DateTime> AddBusinessTimeAsync(DateTime start, TimeSpan duration)
    {
        var remaining = duration;
        var current = await SnapToNextBusinessMomentAsync(start);

        while (remaining > TimeSpan.Zero)
        {
            var slot = await GetCurrentSlotAsync(current);
            var slotEnd = current.Date + slot.End;
            var availableInSlot = slotEnd - current;

            if (remaining <= availableInSlot)
            {
                current = current.Add(remaining);
                remaining = TimeSpan.Zero;
            }
            else
            {
                // On consomme tout le créneau courant, on saute au prochain
                // (qui peut être l'après-midi, ou le lendemain matin)
                remaining -= availableInSlot;
                current = await SnapToNextBusinessMomentAsync(slotEnd);
            }
        }

        return current;
    }

    // Calcule le temps OUVRÉ écoulé entre deux dates
    public async Task<TimeSpan> GetElapsedBusinessTimeAsync(DateTime start, DateTime end)
    {
        if (end <= start) return TimeSpan.Zero;

        var elapsed = TimeSpan.Zero;
        var currentDate = start.Date;

        while (currentDate <= end.Date)
        {
            if (await IsBusinessDayAsync(currentDate))
            {
                // On parcourt CHAQUE créneau de la journée séparément
                foreach (var slot in _options.DailySlots)
                {
                    var slotStart = currentDate + slot.Start;
                    var slotEnd = currentDate + slot.End;

                    var segmentStart = start > slotStart ? start : slotStart;
                    var segmentEnd = end < slotEnd ? end : slotEnd;

                    if (segmentEnd > segmentStart)
                        elapsed += segmentEnd - segmentStart;
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return elapsed;
    }

    // Trouve le créneau contenant l'instant donné
    private Task<BusinessTimeSlot> GetCurrentSlotAsync(DateTime moment)
    {
        var timeOfDay = moment.TimeOfDay;

        var slot = _options.DailySlots
            .FirstOrDefault(s => timeOfDay >= s.Start && timeOfDay < s.End);

        if (slot is null)
            throw new InvalidOperationException(
                $"L'instant {moment} n'appartient à aucun créneau ouvré.");

        return Task.FromResult(slot);
    }

    // Recale une date vers le prochain instant "ouvré" valide
    // (ex: un samedi 10h → lundi 9h ; un lundi 20h → mardi 9h)
    private async Task<DateTime> SnapToNextBusinessMomentAsync(DateTime date)
    {
        var current = date;

        while (true)
        {
            if (!await IsBusinessDayAsync(current.Date))
            {
                current = current.Date.AddDays(1) + _options.DailySlots.First().Start;
                continue;
            }

            var timeOfDay = current.TimeOfDay;

            // Avant le premier créneau (ex: 7h du matin) → snap au début du 1er créneau
            if (timeOfDay < _options.DailySlots.First().Start)
                return current.Date + _options.DailySlots.First().Start;

            // Après le dernier créneau (ex: 18h) → jour ouvré suivant
            if (timeOfDay >= _options.DailySlots.Last().End)
            {
                current = current.Date.AddDays(1) + _options.DailySlots.First().Start;
                continue;
            }

            // Vérifie si on est DANS un créneau valide
            var matchingSlot = _options.DailySlots
                .FirstOrDefault(s => timeOfDay >= s.Start && timeOfDay < s.End);

            if (matchingSlot is not null)
                return current;

            // On est dans un TROU entre deux créneaux (ex: 12h30, pendant la pause)
            // → snap vers le DÉBUT du prochain créneau
            var nextSlot = _options.DailySlots
                .FirstOrDefault(s => s.Start > timeOfDay);

            if (nextSlot is not null)
                return current.Date + nextSlot.Start;

            // Sécurité (ne devrait pas arriver vu les checks précédents)
            current = current.Date.AddDays(1) + _options.DailySlots.First().Start;
        }
    }
}
