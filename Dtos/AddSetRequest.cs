namespace WorkoutTracker.Dtos;

public class AddSetRequest
{
    // which session & exercise
    public int WorkoutSessionId { get; set; }
    public int ExerciseId { get; set; }
    
    // set details
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
}