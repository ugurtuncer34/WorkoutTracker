namespace WorkoutTracker.Entities;

public class WorkoutTemplateExercise : BaseEntity
{
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int Position { get; set; }
    public int TargetSetCount { get; set; }
    public int? RepMin { get; set; }
    public int? RepMax { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public decimal? SuggestedWeightKg { get; set; }
    public string? Notes { get; set; }
    public bool IsOptional { get; set; }
}
