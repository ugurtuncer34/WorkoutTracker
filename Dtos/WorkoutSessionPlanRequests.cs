using WorkoutTracker.Entities;

namespace WorkoutTracker.Dtos;

public class StartSessionFromTemplateRequest
{
    public int WorkoutTemplateId { get; set; }
    public string? Notes { get; set; }
}

public class AddWorkoutSessionExerciseRequest
{
    public int ExerciseId { get; set; }
    public int? Position { get; set; }
    public int PlannedSetCount { get; set; }
    public int? RepMin { get; set; }
    public int? RepMax { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public decimal? SuggestedWeightKg { get; set; }
    public string? Notes { get; set; }
    public bool IsOptional { get; set; }
}

public class UpdateWorkoutSessionExerciseStatusRequest
{
    public WorkoutSessionExerciseStatus Status { get; set; }
}
