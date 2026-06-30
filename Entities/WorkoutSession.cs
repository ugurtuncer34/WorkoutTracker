namespace WorkoutTracker.Entities;

public class WorkoutSession : BaseEntity
{
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; } = false;

    // one to many
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
}