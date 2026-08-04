using WorkoutTracker.Entities;

namespace WorkoutTracker.Dtos;

public class WorkoutTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public WorkoutTemplateCategory Category { get; set; }
    public string? Notes { get; set; }
    public List<WorkoutTemplateExerciseRequest> Exercises { get; set; } = new();
}

public class WorkoutTemplateExerciseRequest
{
    public int ExerciseId { get; set; }
    public int Position { get; set; }
    public int TargetSetCount { get; set; }
    public int? RepMin { get; set; }
    public int? RepMax { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public decimal? SuggestedWeightKg { get; set; }
    public string? Notes { get; set; }
    public bool IsOptional { get; set; }
}

public class CloneWorkoutTemplateRequest
{
    public string? Name { get; set; }
}
