namespace WorkoutTracker.Entities;

public class SetLog : BaseEntity
{
    
    public int SetNumber { get; set; }
    
    public int? Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Notes { get; set; }

    // foreign keys
    public int WorkoutSessionId { get; set; }
    public WorkoutSession WorkoutSession { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
}