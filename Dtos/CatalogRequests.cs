using WorkoutTracker.Entities;

namespace WorkoutTracker.Dtos;

public class CreateMuscleGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
}

public class CreateTargetMuscleRequest
{
    public int MuscleGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
}

public class CreateExerciseRequest
{
    public int TargetMuscleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
    public ExerciseType Type { get; set; } = ExerciseType.RepsAndWeight;
}

public class UpdateMuscleGroupRequest
{
    public string? Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
}

public class UpdateTargetMuscleRequest
{
    public int? MuscleGroupId { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
}

public class UpdateExerciseRequest
{
    public int? TargetMuscleId { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
    public ExerciseType? Type { get; set; }
}