namespace WorkoutTracker.Entities;

public class WorkoutSession : BaseEntity
{
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; } = false;
    
    // Foreign Key
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? WorkoutTemplateId { get; set; }
    public WorkoutTemplate? WorkoutTemplate { get; set; }
    public string? TemplateNameSnapshot { get; set; }
    public WorkoutTemplateCategory? TemplateCategorySnapshot { get; set; }

    // one to many
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
    public ICollection<WorkoutSessionExercise> Exercises { get; set; } = new List<WorkoutSessionExercise>();
}
