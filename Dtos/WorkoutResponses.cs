namespace WorkoutTracker.Dtos;

public class WorkoutSessionResponse
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    
    // Total volume as Weight * Rep
    public decimal TotalVolumeKg => Exercises.Sum(e => e.Sets.Sum(s => (s.Reps ?? 0) * (s.WeightKg ?? 0m)));

    public List<ExerciseLogResponse> Exercises { get; set; } = new();
}

public class ExerciseLogResponse
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string? ExerciseIconKey { get; set; }
    public string TargetMuscleName { get; set; } = string.Empty;
    public string MuscleGroupName { get; set; } = string.Empty;
    public string? MuscleGroupIconKey { get; set; }
    public List<SetLogResponse> Sets { get; set; } = new();
}

public class SetLogResponse
{
    public int Id { get; set; }
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Notes { get; set; }
}

public class StartSessionResponse
{
    public int SessionId { get; set; }
}

public class AddSetResponse
{
    public int LogId { get; set; }
}

public class LastPerformanceResponse
{
    public int WorkoutSessionId { get; set; }
    public DateTime SessionCreatedAt { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public List<LastPerformanceSetResponse> Sets { get; set; } = new();
}

public class LastPerformanceSetResponse
{
    public int Id { get; set; }
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Notes { get; set; }
}
