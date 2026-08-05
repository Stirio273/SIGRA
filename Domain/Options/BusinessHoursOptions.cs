namespace SIGRA.Domain.Options;

public record BusinessTimeSlot(TimeSpan Start, TimeSpan End)
{
    public TimeSpan Duration => End - Start;
}

public class BusinessHoursOptions
{
    // Plusieurs créneaux au lieu d'un seul Start/End
    public List<BusinessTimeSlot> DailySlots { get; set; } = new()
    {
        new BusinessTimeSlot(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)),
        new BusinessTimeSlot(new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0))
    };

    public DayOfWeek[] WorkingDays { get; set; } =
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday
    };

    // Durée ouvrée totale d'une journée = somme des créneaux
    public TimeSpan DailyDuration =>
        DailySlots.Aggregate(TimeSpan.Zero, (sum, slot) => sum + slot.Duration);
}
