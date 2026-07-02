namespace WorkoutTracker.Entities;

public class WorkoutSession : BaseEntity
{
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; } = false;
    
    // Foreign Key
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // one to many
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
}