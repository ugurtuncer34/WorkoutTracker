namespace WorkoutTracker.Dtos;

public class WorkoutTemplateListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkoutTemplateExerciseSummaryResponse> Exercises { get; set; } = new();
}

public class WorkoutTemplateExerciseSummaryResponse
{
    public int TemplateExerciseId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public string? IconKey { get; set; }
    public int Position { get; set; }
}

public class WorkoutTemplateResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<WorkoutTemplateExerciseResponse> Exercises { get; set; } = new();
}

public class WorkoutTemplateExerciseResponse : WorkoutTemplateExerciseSummaryResponse
{
    public int TargetMuscleId { get; set; }
    public string TargetMuscleName { get; set; } = string.Empty;
    public int MuscleGroupId { get; set; }
    public string MuscleGroupName { get; set; } = string.Empty;
    public int TargetSetCount { get; set; }
    public int? RepMin { get; set; }
    public int? RepMax { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public decimal? SuggestedWeightKg { get; set; }
    public string? Notes { get; set; }
    public bool IsOptional { get; set; }
}
