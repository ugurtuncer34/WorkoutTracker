using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Data;
using WorkoutTracker.Dtos;
using WorkoutTracker.Entities;

namespace WorkoutTracker.Services;

public class WorkoutService : IWorkoutService
{
    private readonly AppDbContext _context;

    public WorkoutService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<StartSessionResponse>> StartSessionAsync(StartSessionRequest request)
    {
        var newSession = new WorkoutSession
        {
            Notes = request.Notes,
            IsCompleted = false
        };

        _context.WorkoutSessions.Add(newSession);
        await _context.SaveChangesAsync();

        return new ServiceResponse<StartSessionResponse>
        {
            Data = new StartSessionResponse { SessionId = newSession.Id },
            Message = "Session started successfully."
        };
    }

    public async Task<ServiceResponse<AddSetResponse>> AddSetAsync(AddSetRequest request)
    {
        var sessionExists = await _context.WorkoutSessions.AnyAsync(s => s.Id == request.WorkoutSessionId);
        if (!sessionExists)
        {
            return new ServiceResponse<AddSetResponse> { Success = false, IsNotFound = true, Message = "Workout session not found." };
        }

        var newSet = new SetLog
        {
            WorkoutSessionId = request.WorkoutSessionId,
            ExerciseId = request.ExerciseId,
            SetNumber = request.SetNumber,
            Reps = request.Reps,
            WeightKg = request.WeightKg
        };

        _context.SetLogs.Add(newSet);
        await _context.SaveChangesAsync();

        return new ServiceResponse<AddSetResponse>
        {
            Data = new AddSetResponse { LogId = newSet.Id },
            Message = "Set added successfully."
        };
    }

    public async Task<ServiceResponse<bool>> CompleteSessionAsync(int sessionId)
    {
        var session = await _context.WorkoutSessions.FindAsync(sessionId);
        if (session is null)
        {
            return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Workout session not found." };
        }

        session.IsCompleted = true;
        await _context.SaveChangesAsync();

        return new ServiceResponse<bool> { Data = true, Message = "Session marked as completed." };
    }

    public async Task<ServiceResponse<List<WorkoutSessionResponse>>> GetAllSessionsAsync()
    {
        var sessions = await _context.WorkoutSessions
            .Include(s => s.SetLogs)
                .ThenInclude(sl => sl.Exercise)
                    .ThenInclude(e => e.TargetMuscle)
                        .ThenInclude(tm => tm.MuscleGroup)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var response = sessions.Select(s => new WorkoutSessionResponse
        {
            Id = s.Id,
            Date = s.CreatedAt,
            Notes = s.Notes,
            IsCompleted = s.IsCompleted,
            Exercises = s.SetLogs
                .GroupBy(sl => sl.Exercise)
                .Select(g => new ExerciseLogResponse
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
                        WeightKg = sl.WeightKg
                    }).ToList()
                }).ToList()
        }).ToList();

        return new ServiceResponse<List<WorkoutSessionResponse>> { Data = response };
    }

    public async Task<ServiceResponse<WorkoutSessionResponse>> GetSessionByIdAsync(int id)
    {
        var session = await _context.WorkoutSessions
            .Include(s => s.SetLogs)
                .ThenInclude(sl => sl.Exercise)
                    .ThenInclude(e => e.TargetMuscle)
                        .ThenInclude(tm => tm.MuscleGroup)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
        {
            return new ServiceResponse<WorkoutSessionResponse> { Success = false, IsNotFound = true, Message = "Workout session not found." };
        }

        var response = new WorkoutSessionResponse
        {
            Id = session.Id,
            Date = session.CreatedAt,
            Notes = session.Notes,
            IsCompleted = session.IsCompleted,
            Exercises = session.SetLogs
                .GroupBy(sl => sl.Exercise)
                .Select(g => new ExerciseLogResponse
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
                        WeightKg = sl.WeightKg
                    }).ToList()
                }).ToList()
        };

        return new ServiceResponse<WorkoutSessionResponse> { Data = response };
    }

    public async Task<ServiceResponse<bool>> CancelSessionAsync(int sessionId)
    {
        var session = await _context.WorkoutSessions.FindAsync(sessionId);
        if (session == null)
        {
            return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Workout session not found." };
        }

        if (session.IsCompleted)
        {
            return new ServiceResponse<bool> { Success = false, Message = "Cannot cancel a completed session." };
        }

        _context.WorkoutSessions.Remove(session);
        await _context.SaveChangesAsync();

        return new ServiceResponse<bool> { Data = true, Message = "Session cancelled and deleted successfully." };
    }

    public async Task<ServiceResponse<bool>> DeleteSetAsync(int logId)
    {
        var setLog = await _context.SetLogs.FindAsync(logId);
        if (setLog == null)
        {
            return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Set not found." };
        }

        _context.SetLogs.Remove(setLog);
        await _context.SaveChangesAsync();

        return new ServiceResponse<bool> { Data = true, Message = "Set deleted successfully." };
    }
}