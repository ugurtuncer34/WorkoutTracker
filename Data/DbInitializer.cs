using System.Linq;
using WorkoutTracker.Entities;

namespace WorkoutTracker.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.MuscleGroups.Any())
            {
                return;
            }

            var arms = new MuscleGroup { Name = "Arms", IconKey = "ic_arms" };
            var chest = new MuscleGroup { Name = "Chest", IconKey = "ic_chest" };
            var back = new MuscleGroup { Name = "Back", IconKey = "ic_back" };
            var shoulders = new MuscleGroup { Name = "Shoulders", IconKey = "ic_shoulders" };
            var legs = new MuscleGroup { Name = "Legs", IconKey = "ic_legs" };
            var core = new MuscleGroup { Name = "Core", IconKey = "ic_core" };

            context.MuscleGroups.AddRange(arms, chest, back, shoulders, legs, core);
            context.SaveChanges();

            var biceps = new TargetMuscle { Name = "Biceps", IconKey = "ic_biceps", MuscleGroupId = arms.Id };
            var triceps = new TargetMuscle { Name = "Triceps", IconKey = "ic_triceps", MuscleGroupId = arms.Id };

            var pectorals = new TargetMuscle { Name = "Pectorals", IconKey = "ic_pectorals", MuscleGroupId = chest.Id };

            var lats = new TargetMuscle { Name = "Lats", IconKey = "ic_lats", MuscleGroupId = back.Id };

            var anteriorDelt = new TargetMuscle { Name = "Anterior Deltoid", IconKey = "ic_anterior_deltoid", MuscleGroupId = shoulders.Id };
            var lateralDelt = new TargetMuscle { Name = "Lateral Deltoid", IconKey = "ic_lateral_deltoid", MuscleGroupId = shoulders.Id };
            var posteriorDelt = new TargetMuscle { Name = "Posterior Deltoid", IconKey = "ic_posterior_deltoid", MuscleGroupId = shoulders.Id };

            var quadriceps = new TargetMuscle { Name = "Quadriceps", IconKey = "ic_quadriceps", MuscleGroupId = legs.Id };
            var hamstrings = new TargetMuscle { Name = "Hamstrings", IconKey = "ic_hamstrings", MuscleGroupId = legs.Id };

            var abs = new TargetMuscle { Name = "Abs", IconKey = "ic_abs", MuscleGroupId = core.Id };

            var glutes = new TargetMuscle { Name = "Glutes", IconKey = "ic_glutes", MuscleGroupId = core.Id };

            context.TargetMuscles.AddRange(biceps, triceps, pectorals, lats, anteriorDelt, lateralDelt, posteriorDelt, quadriceps, hamstrings, glutes, abs);
            context.SaveChanges();

            var exercises = new[]
            {
                // Arms & Chest (Default Type is RepsAndWeight)
                new Exercise { Name = "Biceps Curl", IconKey = "ic_biceps_curl", TargetMuscleId = biceps.Id },
                new Exercise { Name = "Supinated Biceps Curl", IconKey = "ic_supinated_biceps_curl", TargetMuscleId = biceps.Id },
                new Exercise { Name = "Hammer Curl", IconKey = "ic_hammer_curl", TargetMuscleId = biceps.Id },
                new Exercise { Name = "Preacher Curl", IconKey = "ic_preacher_curl", TargetMuscleId = biceps.Id },
                new Exercise { Name = "Concentration Curl", IconKey = "ic_concentration_curl", TargetMuscleId = biceps.Id },
                new Exercise { Name = "Skullcrusher", IconKey = "ic_skullcrusher", TargetMuscleId = triceps.Id },
                new Exercise { Name = "Triceps Kickback", IconKey = "ic_triceps_kickback", TargetMuscleId = triceps.Id },
                new Exercise { Name = "Overhead Triceps Extension", IconKey = "ic_overhead_triceps_extension", TargetMuscleId = triceps.Id },
                new Exercise { Name = "Dumbbell Chest Press", IconKey = "ic_dumbbell_chest_press", TargetMuscleId = pectorals.Id },
                new Exercise { Name = "Dumbbell Chest Fly", IconKey = "ic_dumbbell_chest_fly", TargetMuscleId = pectorals.Id },
                new Exercise { Name = "Push-Up", IconKey = "ic_push_up", TargetMuscleId = pectorals.Id, Type = ExerciseType.RepsOnly }, // Pushup is bodyweight

                // Back (Split Rows)
                new Exercise { Name = "Pronated Bent-Over Row", IconKey = "ic_pronated_bent_over_row", TargetMuscleId = lats.Id },
                new Exercise { Name = "Neutral Grip Bent-Over Row", IconKey = "ic_neutral_grip_bent_over_row", TargetMuscleId = lats.Id },
                new Exercise { Name = "Dumbbell Pullover", IconKey = "ic_dumbbell_pullover", TargetMuscleId = lats.Id },

                // Shoulders
                new Exercise { Name = "Dumbbell Shoulder Press", IconKey = "ic_dumbbell_shoulder_press", TargetMuscleId = anteriorDelt.Id },
                new Exercise { Name = "Lateral Raise", IconKey = "ic_lateral_raise", TargetMuscleId = lateralDelt.Id },
                new Exercise { Name = "Rear Delt Fly", IconKey = "ic_rear_delt_fly", TargetMuscleId = posteriorDelt.Id },

                // Legs & Core (Assigned specific types)
                new Exercise { Name = "Squat", IconKey = "ic_squat", TargetMuscleId = quadriceps.Id, Type = ExerciseType.RepsOnly },
                new Exercise { Name = "Romanian Deadlift", IconKey = "ic_romanian_deadlift", TargetMuscleId = hamstrings.Id },
                new Exercise { Name = "Glute Bridge", IconKey = "ic_glute_bridge", TargetMuscleId = glutes.Id, Type = ExerciseType.RepsOnly },
                new Exercise { Name = "Forearm Plank", IconKey = "ic_forearm_plank", TargetMuscleId = abs.Id, Type = ExerciseType.DurationOnly }
            };

            context.Exercises.AddRange(exercises);
            context.SaveChanges();
        }
    }
}