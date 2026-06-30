namespace WorkoutTracker.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconKey { get; set; }

    public int TargetMuscleId { get; set; }
    public TargetMuscle TargetMuscle { get; set; } = null!;
    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
}