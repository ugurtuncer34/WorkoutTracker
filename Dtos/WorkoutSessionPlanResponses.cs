namespace WorkoutTracker.Dtos;

public class WorkoutSessionExercisePlanResponse
{
    public int WorkoutSessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public int? WorkoutTemplateId { get; set; }
    public string? TemplateNameSnapshot { get; set; }
    public string? TemplateCategorySnapshot { get; set; }
    public bool IsTemplateSession => WorkoutTemplateId.HasValue || TemplateNameSnapshot is not null || TemplateCategorySnapshot is not null;
    public int TotalExerciseCount => Exercises.Count;
    public int CompletedExerciseCount => Exercises.Count(e => e.Status == nameof(Entities.WorkoutSessionExerciseStatus.Completed));
    public int SkippedExerciseCount => Exercises.Count(e => e.Status == nameof(Entities.WorkoutSessionExerciseStatus.Skipped));
    public List<WorkoutSessionExerciseResponse> Exercises { get; set; } = new();
}

public class WorkoutSessionExerciseResponse
{
    public int SessionExerciseId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public string? IconKey { get; set; }
    public string TargetMuscleName { get; set; } = string.Empty;
    public string MuscleGroupName { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsAdHoc { get; set; }
    public int PlannedSetCount { get; set; }
    public int CompletedSetCount => Sets.Count;
    public int? RepMin { get; set; }
    public int? RepMax { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public decimal? SuggestedWeightKg { get; set; }
    public string? Notes { get; set; }
    public bool IsOptional { get; set; }
    public List<SetLogResponse> Sets { get; set; } = new();
}
