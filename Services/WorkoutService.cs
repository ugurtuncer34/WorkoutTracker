using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Data;
using WorkoutTracker.Dtos;
using WorkoutTracker.Entities;

namespace WorkoutTracker.Services;

public class WorkoutService : IWorkoutService
{
    private const int ExerciseNotesMaxLength = 500;
    private readonly AppDbContext _context;

    public WorkoutService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<StartSessionResponse>> StartSessionAsync(StartSessionRequest request, int userId)
    {
        var newSession = new WorkoutSession { Notes = request.Notes, IsCompleted = false, UserId = userId };
        _context.WorkoutSessions.Add(newSession);
        await _context.SaveChangesAsync();
        return new ServiceResponse<StartSessionResponse> { Data = new StartSessionResponse { SessionId = newSession.Id } };
    }

    public async Task<ServiceResponse<WorkoutSessionExercisePlanResponse>> StartSessionFromTemplateAsync(
        StartSessionFromTemplateRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var template = await _context.WorkoutTemplates
            .AsNoTracking()
            .Where(t => t.Id == request.WorkoutTemplateId && t.UserId == userId)
            .Include(t => t.Exercises)
                .ThenInclude(te => te.Exercise)
                    .ThenInclude(e => e.TargetMuscle)
                        .ThenInclude(tm => tm.MuscleGroup)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return PlanNotFound("Workout template not found.");
        }

        if (template.IsArchived)
        {
            return PlanValidationFailure("Archived workout templates cannot be used to start a session.");
        }

        var now = DateTime.UtcNow;
        var session = new WorkoutSession
        {
            UserId = userId,
            Notes = request.Notes,
            IsCompleted = false,
            WorkoutTemplateId = template.Id,
            TemplateNameSnapshot = template.Name,
            TemplateCategorySnapshot = template.Category,
            CreatedAt = now,
            Exercises = template.Exercises
                .OrderBy(e => e.Position)
                .ThenBy(e => e.Id)
                .Select((e, index) => new WorkoutSessionExercise
                {
                    ExerciseId = e.ExerciseId,
                    Position = index + 1,
                    Status = WorkoutSessionExerciseStatus.Planned,
                    IsAdHoc = false,
                    PlannedSetCount = e.TargetSetCount,
                    RepMin = e.RepMin,
                    RepMax = e.RepMax,
                    TargetDurationSeconds = e.TargetDurationSeconds,
                    SuggestedWeightKg = e.SuggestedWeightKg,
                    NotesSnapshot = e.Notes,
                    IsOptional = e.IsOptional,
                    ExerciseNameSnapshot = e.Exercise.Name,
                    ExerciseTypeSnapshot = e.Exercise.Type,
                    IconKeySnapshot = e.Exercise.IconKey,
                    TargetMuscleNameSnapshot = e.Exercise.TargetMuscle.Name,
                    MuscleGroupNameSnapshot = e.Exercise.TargetMuscle.MuscleGroup.Name,
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToList()
        };

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var response = await LoadExercisePlanAsync(session.Id, userId, cancellationToken);
        return new ServiceResponse<WorkoutSessionExercisePlanResponse>
        {
            Data = response!,
            Message = "Workout session started from template successfully."
        };
    }

    public async Task<ServiceResponse<AddSetResponse>> AddSetAsync(AddSetRequest request, int userId)
    {
        var session = await _context.WorkoutSessions.FirstOrDefaultAsync(s => s.Id == request.WorkoutSessionId && s.UserId == userId);
        if (session == null) return new ServiceResponse<AddSetResponse> { Success = false, IsNotFound = true, Message = "Workout session not found." };

        if (session.IsCompleted)
        {
            var hasExercisePlan =
                session.WorkoutTemplateId.HasValue ||
                session.TemplateNameSnapshot is not null ||
                session.TemplateCategorySnapshot.HasValue ||
                await _context.WorkoutSessionExercises.AnyAsync(e => e.WorkoutSessionId == session.Id);

            if (request.WorkoutSessionExerciseId.HasValue || hasExercisePlan)
            {
                return new ServiceResponse<AddSetResponse> { Success = false, Message = "Cannot add a set to a completed session." };
            }
        }

        WorkoutSessionExercise? sessionExercise = null;
        if (request.WorkoutSessionExerciseId.HasValue)
        {
            sessionExercise = await _context.WorkoutSessionExercises
                .FirstOrDefaultAsync(e =>
                    e.Id == request.WorkoutSessionExerciseId.Value &&
                    e.WorkoutSessionId == request.WorkoutSessionId &&
                    e.WorkoutSession.UserId == userId);

            if (sessionExercise is null)
            {
                return new ServiceResponse<AddSetResponse> { Success = false, IsNotFound = true, Message = "Workout session exercise not found." };
            }

            if (sessionExercise.ExerciseId != request.ExerciseId)
            {
                return new ServiceResponse<AddSetResponse> { Success = false, Message = "ExerciseId does not match the workout session exercise." };
            }

            if (sessionExercise.Status == WorkoutSessionExerciseStatus.Skipped)
            {
                return new ServiceResponse<AddSetResponse> { Success = false, Message = "Cannot add a set to a skipped workout session exercise." };
            }

            if (sessionExercise.Status == WorkoutSessionExerciseStatus.Planned)
            {
                sessionExercise.Status = WorkoutSessionExerciseStatus.InProgress;
                sessionExercise.UpdatedAt = DateTime.UtcNow;
            }
        }

        var newSet = new SetLog
        {
            WorkoutSessionId = request.WorkoutSessionId,
            ExerciseId = request.ExerciseId,
            WorkoutSessionExerciseId = request.WorkoutSessionExerciseId,
            SetNumber = request.SetNumber,
            Reps = request.Reps,
            WeightKg = request.WeightKg,
            DurationSeconds = request.DurationSeconds,
            Notes = request.Notes
        };
        _context.SetLogs.Add(newSet);
        await _context.SaveChangesAsync();
        return new ServiceResponse<AddSetResponse> { Data = new AddSetResponse { LogId = newSet.Id } };
    }

    public async Task<ServiceResponse<WorkoutSessionExercisePlanResponse>> GetExercisePlanAsync(
        int sessionId,
        int userId,
        CancellationToken cancellationToken)
    {
        var response = await LoadExercisePlanAsync(sessionId, userId, cancellationToken);
        return response is null
            ? PlanNotFound("Workout session not found.")
            : new ServiceResponse<WorkoutSessionExercisePlanResponse> { Data = response };
    }

    public async Task<ServiceResponse<WorkoutSessionExercisePlanResponse>> AddExerciseAsync(
        int sessionId,
        AddWorkoutSessionExerciseRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var session = await _context.WorkoutSessions
            .Include(s => s.Exercises)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session is null)
        {
            return PlanNotFound("Workout session not found.");
        }

        if (session.IsCompleted)
        {
            return PlanValidationFailure("Completed workout sessions cannot be modified.");
        }

        var exercise = await _context.Exercises
            .AsNoTracking()
            .Where(e => e.Id == request.ExerciseId)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.IconKey,
                e.Type,
                TargetMuscleName = e.TargetMuscle.Name,
                MuscleGroupName = e.TargetMuscle.MuscleGroup.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (exercise is null)
        {
            return PlanNotFound("Exercise not found.");
        }

        var validationMessage = ValidateExerciseRequest(request, exercise.Type, session.Exercises.Count);
        if (validationMessage.Length > 0)
        {
            return PlanValidationFailure(validationMessage);
        }

        var insertPosition = request.Position ?? session.Exercises.Count + 1;
        var orderedExercises = session.Exercises
            .OrderBy(e => e.Position)
            .ThenBy(e => e.Id)
            .ToList();
        var now = DateTime.UtcNow;

        foreach (var existing in orderedExercises)
        {
            existing.Position = -existing.Position;
            existing.UpdatedAt = now;
        }

        if (orderedExercises.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        var newExercise = new WorkoutSessionExercise
        {
            WorkoutSessionId = session.Id,
            ExerciseId = exercise.Id,
            Position = insertPosition,
            Status = WorkoutSessionExerciseStatus.Planned,
            IsAdHoc = true,
            PlannedSetCount = request.PlannedSetCount,
            RepMin = request.RepMin,
            RepMax = request.RepMax,
            TargetDurationSeconds = request.TargetDurationSeconds,
            SuggestedWeightKg = request.SuggestedWeightKg,
            NotesSnapshot = NormalizeNotes(request.Notes),
            IsOptional = request.IsOptional,
            ExerciseNameSnapshot = exercise.Name,
            ExerciseTypeSnapshot = exercise.Type,
            IconKeySnapshot = exercise.IconKey,
            TargetMuscleNameSnapshot = exercise.TargetMuscleName,
            MuscleGroupNameSnapshot = exercise.MuscleGroupName,
            CreatedAt = now,
            UpdatedAt = now
        };

        var existingIndex = 0;
        for (var position = 1; position <= orderedExercises.Count + 1; position++)
        {
            if (position == insertPosition) continue;
            orderedExercises[existingIndex++].Position = position;
        }

        _context.WorkoutSessionExercises.Add(newExercise);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var response = await LoadExercisePlanAsync(session.Id, userId, cancellationToken);
        return new ServiceResponse<WorkoutSessionExercisePlanResponse>
        {
            Data = response!,
            Message = "Exercise added to workout session successfully."
        };
    }

    public async Task<ServiceResponse<WorkoutSessionExerciseResponse>> UpdateExerciseStatusAsync(
        int sessionId,
        int sessionExerciseId,
        UpdateWorkoutSessionExerciseStatusRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
        {
            return ExerciseValidationFailure("Status is invalid.");
        }

        var sessionExercise = await _context.WorkoutSessionExercises
            .Include(e => e.WorkoutSession)
            .Include(e => e.SetLogs)
            .FirstOrDefaultAsync(e =>
                e.Id == sessionExerciseId &&
                e.WorkoutSessionId == sessionId &&
                e.WorkoutSession.UserId == userId,
                cancellationToken);

        if (sessionExercise is null)
        {
            return new ServiceResponse<WorkoutSessionExerciseResponse>
            {
                Success = false,
                IsNotFound = true,
                Message = "Workout session exercise not found."
            };
        }

        if (sessionExercise.WorkoutSession.IsCompleted)
        {
            return ExerciseValidationFailure("Completed workout sessions cannot be modified.");
        }

        if (!IsAllowedTransition(sessionExercise.Status, request.Status))
        {
            return ExerciseValidationFailure($"Status cannot change from {sessionExercise.Status} to {request.Status}.");
        }

        sessionExercise.Status = request.Status;
        sessionExercise.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResponse<WorkoutSessionExerciseResponse>
        {
            Data = MapSessionExercise(sessionExercise),
            Message = "Workout session exercise status updated successfully."
        };
    }

    public async Task<ServiceResponse<bool>> CompleteSessionAsync(int sessionId, int userId)
    {
        var session = await _context.WorkoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session is null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Workout session not found." };

        session.IsCompleted = true;
        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true };
    }

    public async Task<ServiceResponse<List<WorkoutSessionResponse>>> GetAllSessionsAsync(int userId)
    {
        var sessions = await _context.WorkoutSessions
            .Where(s => s.UserId == userId)
            .Include(s => s.SetLogs).ThenInclude(sl => sl.Exercise).ThenInclude(e => e.TargetMuscle).ThenInclude(tm => tm.MuscleGroup)
            .OrderByDescending(s => s.CreatedAt).ToListAsync();

        var response = sessions.Select(MapWorkoutSession).ToList();
        return new ServiceResponse<List<WorkoutSessionResponse>> { Data = response };
    }

    public async Task<ServiceResponse<WorkoutSessionResponse>> GetSessionByIdAsync(int id, int userId)
    {
        var session = await _context.WorkoutSessions
            .Include(s => s.SetLogs).ThenInclude(sl => sl.Exercise).ThenInclude(e => e.TargetMuscle).ThenInclude(tm => tm.MuscleGroup)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (session == null) return new ServiceResponse<WorkoutSessionResponse> { Success = false, IsNotFound = true, Message = "Workout session not found." };
        return new ServiceResponse<WorkoutSessionResponse> { Data = MapWorkoutSession(session) };
    }

    public async Task<ServiceResponse<LastPerformanceResponse?>> GetLastPerformanceAsync(
        int exerciseId,
        int userId,
        int? excludeSessionId,
        CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .AsNoTracking()
            .Where(e => e.Id == exerciseId)
            .Select(e => new { e.Id, e.Name, e.Type })
            .FirstOrDefaultAsync(cancellationToken);

        if (exercise is null)
        {
            return new ServiceResponse<LastPerformanceResponse?>
            {
                Success = false,
                IsNotFound = true,
                Message = "Exercise not found."
            };
        }

        var performance = await _context.WorkoutSessions
            .AsNoTracking()
            .Where(session =>
                session.UserId == userId &&
                session.IsCompleted &&
                (!excludeSessionId.HasValue || session.Id != excludeSessionId.Value) &&
                session.SetLogs.Any(set => set.ExerciseId == exerciseId))
            .OrderByDescending(session => session.CreatedAt)
            .ThenByDescending(session => session.Id)
            .Select(session => new LastPerformanceResponse
            {
                WorkoutSessionId = session.Id,
                SessionCreatedAt = session.CreatedAt,
                ExerciseId = exercise.Id,
                ExerciseName = exercise.Name,
                ExerciseType = exercise.Type.ToString(),
                Sets = session.SetLogs
                    .Where(set => set.ExerciseId == exerciseId)
                    .OrderBy(set => set.SetNumber)
                    .Select(set => new LastPerformanceSetResponse
                    {
                        Id = set.Id,
                        SetNumber = set.SetNumber,
                        Reps = set.Reps,
                        WeightKg = set.WeightKg,
                        DurationSeconds = set.DurationSeconds,
                        Notes = set.Notes
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ServiceResponse<LastPerformanceResponse?> { Data = performance };
    }

    public async Task<ServiceResponse<bool>> CancelSessionAsync(int sessionId, int userId)
    {
        var session = await _context.WorkoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Workout session not found." };
        if (session.IsCompleted) return new ServiceResponse<bool> { Success = false, Message = "Cannot cancel a completed session." };

        await DeleteSessionGraphAsync(session);
        return new ServiceResponse<bool> { Data = true };
    }

    public async Task<ServiceResponse<bool>> DeleteSessionAsync(int sessionId, int userId)
    {
        var session = await _context.WorkoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Workout session not found." };

        await DeleteSessionGraphAsync(session);
        return new ServiceResponse<bool> { Data = true, Message = "Session permanently deleted." };
    }

    public async Task<ServiceResponse<bool>> DeleteSetAsync(int logId, int userId)
    {
        var setLog = await _context.SetLogs.Include(s => s.WorkoutSession).FirstOrDefaultAsync(s => s.Id == logId);
        if (setLog == null || setLog.WorkoutSession.UserId != userId) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Set not found." };

        _context.SetLogs.Remove(setLog);
        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true };
    }

    private async Task<WorkoutSessionExercisePlanResponse?> LoadExercisePlanAsync(
        int sessionId,
        int userId,
        CancellationToken cancellationToken)
    {
        var session = await _context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Include(s => s.Exercises)
                .ThenInclude(e => e.SetLogs)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null) return null;

        return new WorkoutSessionExercisePlanResponse
        {
            WorkoutSessionId = session.Id,
            CreatedAt = session.CreatedAt,
            IsCompleted = session.IsCompleted,
            Notes = session.Notes,
            WorkoutTemplateId = session.WorkoutTemplateId,
            TemplateNameSnapshot = session.TemplateNameSnapshot,
            TemplateCategorySnapshot = session.TemplateCategorySnapshot?.ToString(),
            Exercises = session.Exercises
                .OrderBy(e => e.Position)
                .ThenBy(e => e.Id)
                .Select(MapSessionExercise)
                .ToList()
        };
    }

    private async Task DeleteSessionGraphAsync(WorkoutSession session)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.SetLogs
            .Where(set => set.WorkoutSessionId == session.Id)
            .ExecuteDeleteAsync();
        await _context.WorkoutSessionExercises
            .Where(exercise => exercise.WorkoutSessionId == session.Id)
            .ExecuteDeleteAsync();
        _context.WorkoutSessions.Remove(session);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static string ValidateExerciseRequest(
        AddWorkoutSessionExerciseRequest request,
        ExerciseType exerciseType,
        int existingExerciseCount)
    {
        var errors = new List<string>();
        if (request.Position is <= 0) errors.Add("Position must be positive when provided.");
        if (request.Position > existingExerciseCount + 1) errors.Add($"Position must not exceed {existingExerciseCount + 1}.");
        if (request.PlannedSetCount <= 0) errors.Add("PlannedSetCount must be greater than zero.");
        if (request.RepMin is <= 0) errors.Add("RepMin must be positive when provided.");
        if (request.RepMax is <= 0) errors.Add("RepMax must be positive when provided.");
        if (request.RepMin.HasValue && request.RepMax.HasValue && request.RepMin > request.RepMax)
            errors.Add("RepMin must not exceed RepMax.");
        if (request.TargetDurationSeconds is <= 0)
            errors.Add("TargetDurationSeconds must be positive when provided.");
        if (request.SuggestedWeightKg < 0)
            errors.Add("SuggestedWeightKg must not be negative.");
        if (request.Notes?.Trim().Length > ExerciseNotesMaxLength)
            errors.Add($"Notes must not exceed {ExerciseNotesMaxLength} characters.");

        switch (exerciseType)
        {
            case ExerciseType.RepsAndWeight:
            case ExerciseType.RepsWithOptionalWeight:
                if (request.TargetDurationSeconds.HasValue)
                    errors.Add("TargetDurationSeconds must be null for this exercise type.");
                break;
            case ExerciseType.RepsOnly:
                if (request.SuggestedWeightKg.HasValue)
                    errors.Add("SuggestedWeightKg must be null for a RepsOnly exercise.");
                if (request.TargetDurationSeconds.HasValue)
                    errors.Add("TargetDurationSeconds must be null for a RepsOnly exercise.");
                break;
            case ExerciseType.DurationOnly:
                if (request.RepMin.HasValue || request.RepMax.HasValue)
                    errors.Add("RepMin and RepMax must be null for a DurationOnly exercise.");
                if (request.SuggestedWeightKg.HasValue)
                    errors.Add("SuggestedWeightKg must be null for a DurationOnly exercise.");
                break;
        }

        return string.Join(" ", errors);
    }

    private static bool IsAllowedTransition(
        WorkoutSessionExerciseStatus current,
        WorkoutSessionExerciseStatus next)
    {
        return (current, next) switch
        {
            (WorkoutSessionExerciseStatus.Planned, WorkoutSessionExerciseStatus.InProgress) => true,
            (WorkoutSessionExerciseStatus.Planned, WorkoutSessionExerciseStatus.Skipped) => true,
            (WorkoutSessionExerciseStatus.InProgress, WorkoutSessionExerciseStatus.Completed) => true,
            (WorkoutSessionExerciseStatus.InProgress, WorkoutSessionExerciseStatus.Skipped) => true,
            (WorkoutSessionExerciseStatus.Skipped, WorkoutSessionExerciseStatus.Planned) => true,
            (WorkoutSessionExerciseStatus.Completed, WorkoutSessionExerciseStatus.InProgress) => true,
            _ => false
        };
    }

    private static WorkoutSessionExerciseResponse MapSessionExercise(WorkoutSessionExercise exercise)
    {
        return new WorkoutSessionExerciseResponse
        {
            SessionExerciseId = exercise.Id,
            ExerciseId = exercise.ExerciseId,
            ExerciseName = exercise.ExerciseNameSnapshot,
            ExerciseType = exercise.ExerciseTypeSnapshot.ToString(),
            IconKey = exercise.IconKeySnapshot,
            TargetMuscleName = exercise.TargetMuscleNameSnapshot,
            MuscleGroupName = exercise.MuscleGroupNameSnapshot,
            Position = exercise.Position,
            Status = exercise.Status.ToString(),
            IsAdHoc = exercise.IsAdHoc,
            PlannedSetCount = exercise.PlannedSetCount,
            RepMin = exercise.RepMin,
            RepMax = exercise.RepMax,
            TargetDurationSeconds = exercise.TargetDurationSeconds,
            SuggestedWeightKg = exercise.SuggestedWeightKg,
            Notes = exercise.NotesSnapshot,
            IsOptional = exercise.IsOptional,
            Sets = exercise.SetLogs
                .OrderBy(set => set.SetNumber)
                .ThenBy(set => set.Id)
                .Select(set => new SetLogResponse
                {
                    Id = set.Id,
                    SetNumber = set.SetNumber,
                    Reps = set.Reps,
                    WeightKg = set.WeightKg,
                    DurationSeconds = set.DurationSeconds,
                    Notes = set.Notes
                })
                .ToList()
        };
    }

    private static WorkoutSessionResponse MapWorkoutSession(WorkoutSession session)
    {
        return new WorkoutSessionResponse
        {
            Id = session.Id,
            Date = session.CreatedAt,
            Notes = session.Notes,
            IsCompleted = session.IsCompleted,
            Exercises = session.SetLogs.GroupBy(sl => sl.Exercise).Select(g => new ExerciseLogResponse
            {
                ExerciseId = g.Key.Id,
                ExerciseName = g.Key.Name,
                ExerciseIconKey = g.Key.IconKey,
                TargetMuscleName = g.Key.TargetMuscle.Name,
                MuscleGroupName = g.Key.TargetMuscle.MuscleGroup.Name,
                MuscleGroupIconKey = g.Key.TargetMuscle.MuscleGroup.IconKey,
                Sets = g.OrderBy(sl => sl.SetNumber).Select(sl => new SetLogResponse
                {
                    Id = sl.Id,
                    SetNumber = sl.SetNumber,
                    Reps = sl.Reps,
                    WeightKg = sl.WeightKg,
                    DurationSeconds = sl.DurationSeconds,
                    Notes = sl.Notes
                }).ToList()
            }).ToList()
        };
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (notes is null) return null;
        var trimmed = notes.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static ServiceResponse<WorkoutSessionExercisePlanResponse> PlanNotFound(string message) => new()
    {
        Success = false,
        IsNotFound = true,
        Message = message
    };

    private static ServiceResponse<WorkoutSessionExercisePlanResponse> PlanValidationFailure(string message) => new()
    {
        Success = false,
        Message = message
    };

    private static ServiceResponse<WorkoutSessionExerciseResponse> ExerciseValidationFailure(string message) => new()
    {
        Success = false,
        Message = message
    };
}
