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
    public DbSet<WorkoutTemplate> WorkoutTemplates { get; set; }
    public DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises { get; set; }
    public DbSet<WorkoutSessionExercise> WorkoutSessionExercises { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SetLog>()
            .Property(s => s.WeightKg)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<SetLog>(entity =>
        {
            entity.HasIndex(s => s.WorkoutSessionExerciseId);
            entity.HasOne(s => s.WorkoutSessionExercise)
                .WithMany(e => e.SetLogs)
                .HasForeignKey(s => s.WorkoutSessionExerciseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.Property(s => s.TemplateNameSnapshot).HasMaxLength(100);
            entity.HasIndex(s => s.WorkoutTemplateId);
            entity.HasOne(s => s.WorkoutTemplate)
                .WithMany(t => t.WorkoutSessions)
                .HasForeignKey(s => s.WorkoutTemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkoutTemplate>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(100);
            entity.Property(t => t.Notes).HasMaxLength(1000);
            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => new { t.UserId, t.IsArchived });
            entity.HasOne(t => t.User)
                .WithMany(u => u.WorkoutTemplates)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutTemplateExercise>(entity =>
        {
            entity.Property(e => e.SuggestedWeightKg).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => new { e.WorkoutTemplateId, e.Position }).IsUnique();
            entity.HasIndex(e => new { e.WorkoutTemplateId, e.ExerciseId }).IsUnique();
            entity.HasOne(e => e.WorkoutTemplate)
                .WithMany(t => t.Exercises)
                .HasForeignKey(e => e.WorkoutTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Exercise)
                .WithMany(e => e.WorkoutTemplateExercises)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutSessionExercise>(entity =>
        {
            entity.Property(e => e.SuggestedWeightKg).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NotesSnapshot).HasMaxLength(500);
            entity.Property(e => e.ExerciseNameSnapshot).IsRequired();
            entity.Property(e => e.TargetMuscleNameSnapshot).IsRequired();
            entity.Property(e => e.MuscleGroupNameSnapshot).IsRequired();
            entity.HasIndex(e => e.WorkoutSessionId);
            entity.HasIndex(e => new { e.WorkoutSessionId, e.Position }).IsUnique();
            entity.HasIndex(e => new { e.WorkoutSessionId, e.Status });
            entity.HasIndex(e => e.ExerciseId);
            entity.HasIndex(e => new { e.WorkoutSessionId, e.ExerciseId })
                .IsUnique()
                .HasFilter("IsAdHoc = 0");
            entity.HasOne(e => e.WorkoutSession)
                .WithMany(s => s.Exercises)
                .HasForeignKey(e => e.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Exercise)
                .WithMany(e => e.WorkoutSessionExercises)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
