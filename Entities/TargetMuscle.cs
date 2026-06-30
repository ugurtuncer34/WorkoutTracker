namespace WorkoutTracker.Entities;

public class TargetMuscle : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }
    // Foreign Key
    public int MuscleGroupId { get; set; }
    public MuscleGroup MuscleGroup { get; set; } = null!;
    // one to many
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}