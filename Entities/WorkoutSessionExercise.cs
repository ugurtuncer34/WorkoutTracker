namespace WorkoutTracker.Entities;

public class WorkoutSessionExercise : BaseEntity
{
    public int WorkoutSessionId { get; set; }
    public WorkoutSession WorkoutSession { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int Position { get; set; }
    public WorkoutSessionExerciseStatus Status { get; set; }
    public bool IsAdHoc { get; set; }
    public int PlannedSetCount { get; set; }
    public int? RepMin { get; set; }
    public int? RepMax { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public decimal? SuggestedWeightKg { get; set; }
    public string? NotesSnapshot { get; set; }
    public bool IsOptional { get; set; }
    public string ExerciseNameSnapshot { get; set; } = string.Empty;
    public ExerciseType ExerciseTypeSnapshot { get; set; }
    public string? IconKeySnapshot { get; set; }
    public string TargetMuscleNameSnapshot { get; set; } = string.Empty;
    public string MuscleGroupNameSnapshot { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
}
