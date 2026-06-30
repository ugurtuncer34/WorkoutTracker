namespace WorkoutTracker.Entities;

public class MuscleGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // one to many
    public ICollection<TargetMuscle> TargetMuscles { get; set; } = new List<TargetMuscle>();
}