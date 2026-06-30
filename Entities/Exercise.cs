namespace WorkoutTracker.Entities;

public class Exercise
{
    public int Id { get; set; }
    public int TargetMuscleId { get; set; }
    public string Name { get; set; } = string.Empty;

    public TargetMuscle TargetMuscle { get; set; } = null!;
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
}