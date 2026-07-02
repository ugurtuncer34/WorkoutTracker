using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Entities;

namespace WorkoutTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<User> Users { get; set; }
    public DbSet<MuscleGroup> MuscleGroups { get; set; }
    public DbSet<TargetMuscle> TargetMuscles { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<WorkoutSession> WorkoutSessions { get; set; }
    public DbSet<SetLog> SetLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SetLog>()
            .Property(s => s.WeightKg)
            .HasColumnType("decimal(18,2)");
    }
}