namespace WorkoutTracker.Dtos;

public class CatalogItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
}

public class MuscleGroupResponse : CatalogItemResponse
{
    public List<CatalogItemResponse> TargetMuscles { get; set; } = new();
}

public class ExerciseCatalogResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
    public string TargetMuscleName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}