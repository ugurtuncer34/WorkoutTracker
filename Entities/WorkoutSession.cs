namespace WorkoutTracker.Entities;

public class WorkoutSession
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; } = false;

    // one to many
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
}