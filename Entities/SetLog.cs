namespace WorkoutTracker.Entities;

public class SetLog
{
    public int Id { get; set; }
    
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // foreign keys
    public int WorkoutSessionId { get; set; }
    public WorkoutSession WorkoutSession { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
}