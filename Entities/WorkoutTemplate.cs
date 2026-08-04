namespace WorkoutTracker.Entities;

public class WorkoutTemplate : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public WorkoutTemplateCategory Category { get; set; }
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<WorkoutTemplateExercise> Exercises { get; set; } = new List<WorkoutTemplateExercise>();
    public ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
}
