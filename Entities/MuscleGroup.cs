namespace WorkoutTracker.Entities;

public class MuscleGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
    // one to many
    public ICollection<TargetMuscle> TargetMuscles { get; set; } = new List<TargetMuscle>();
}