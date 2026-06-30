namespace WorkoutTracker.Entities;

public class TargetMuscle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Foreign Key
    public int MuscleGroupId { get; set; }
    public MuscleGroup MuscleGroup { get; set; } = null!;
    // one to many
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}