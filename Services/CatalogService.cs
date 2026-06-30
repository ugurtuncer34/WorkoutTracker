using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Data;
using WorkoutTracker.Dtos;
using WorkoutTracker.Entities;

namespace WorkoutTracker.Services;

public class CatalogService : ICatalogService
{
    private readonly AppDbContext _context;

    public CatalogService(AppDbContext context)
    {
        _context = context;
    }

    // create

    public async Task<ServiceResponse<CatalogItemResponse>> CreateMuscleGroupAsync(CreateMuscleGroupRequest request)
    {
        var entity = new MuscleGroup { Name = request.Name, IconKey = request.IconKey };
        _context.MuscleGroups.Add(entity);
        await _context.SaveChangesAsync();

        var data = new CatalogItemResponse { Id = entity.Id, Name = entity.Name, IconKey = entity.IconKey };
        return new ServiceResponse<CatalogItemResponse> { Data = data, Message = "Muscle group created successfully." };
    }

    public async Task<ServiceResponse<CatalogItemResponse>> CreateTargetMuscleAsync(CreateTargetMuscleRequest request)
    {
        var exists = await _context.MuscleGroups.AnyAsync(m => m.Id == request.MuscleGroupId);
        if (!exists) return new ServiceResponse<CatalogItemResponse> { Success = false, IsNotFound = true, Message = "Muscle group not found." };

        var entity = new TargetMuscle { MuscleGroupId = request.MuscleGroupId, Name = request.Name, IconKey = request.IconKey };
        _context.TargetMuscles.Add(entity);
        await _context.SaveChangesAsync();

        var data = new CatalogItemResponse { Id = entity.Id, Name = entity.Name, IconKey = entity.IconKey };
        return new ServiceResponse<CatalogItemResponse> { Data = data, Message = "Target muscle created successfully." };
    }

    public async Task<ServiceResponse<CatalogItemResponse>> CreateExerciseAsync(CreateExerciseRequest request)
    {
        var exists = await _context.TargetMuscles.AnyAsync(t => t.Id == request.TargetMuscleId);
        if (!exists) return new ServiceResponse<CatalogItemResponse> { Success = false, IsNotFound = true, Message = "Target muscle not found." };

        var entity = new Exercise { TargetMuscleId = request.TargetMuscleId, Name = request.Name, IconKey = request.IconKey };
        _context.Exercises.Add(entity);
        await _context.SaveChangesAsync();

        var data = new CatalogItemResponse { Id = entity.Id, Name = entity.Name, IconKey = entity.IconKey };
        return new ServiceResponse<CatalogItemResponse> { Data = data, Message = "Exercise created successfully." };
    }

    // update

    public async Task<ServiceResponse<bool>> UpdateMuscleGroupAsync(int id, UpdateMuscleGroupRequest request)
    {
        var entity = await _context.MuscleGroups.FindAsync(id);
        if (entity == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Muscle group not found." };

        if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
        if (request.IconKey != null) entity.IconKey = request.IconKey == "" ? null : request.IconKey;

        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true, Message = "Muscle group updated successfully." };
    }

    public async Task<ServiceResponse<bool>> UpdateTargetMuscleAsync(int id, UpdateTargetMuscleRequest request)
    {
        var entity = await _context.TargetMuscles.FindAsync(id);
        if (entity == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Target muscle not found." };

        if (request.MuscleGroupId.HasValue)
        {
            var exists = await _context.MuscleGroups.AnyAsync(m => m.Id == request.MuscleGroupId.Value);
            if (!exists) return new ServiceResponse<bool> { Success = false, Message = "Invalid MuscleGroupId." };
            entity.MuscleGroupId = request.MuscleGroupId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
        if (request.IconKey != null) entity.IconKey = request.IconKey == "" ? null : request.IconKey;

        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true, Message = "Target muscle updated successfully." };
    }

    public async Task<ServiceResponse<bool>> UpdateExerciseAsync(int id, UpdateExerciseRequest request)
    {
        var entity = await _context.Exercises.FindAsync(id);
        if (entity == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Exercise not found." };

        if (request.TargetMuscleId.HasValue)
        {
            var exists = await _context.TargetMuscles.AnyAsync(t => t.Id == request.TargetMuscleId.Value);
            if (!exists) return new ServiceResponse<bool> { Success = false, Message = "Invalid TargetMuscleId." };
            entity.TargetMuscleId = request.TargetMuscleId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
        if (request.IconKey != null) entity.IconKey = request.IconKey == "" ? null : request.IconKey;

        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true, Message = "Exercise updated successfully." };
    }

    // delete

    public async Task<ServiceResponse<bool>> DeleteMuscleGroupAsync(int id)
    {
        var entity = await _context.MuscleGroups.FindAsync(id);
        if (entity == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Muscle group not found." };

        _context.MuscleGroups.Remove(entity);
        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true, Message = "Muscle group deleted successfully." };
    }

    public async Task<ServiceResponse<bool>> DeleteTargetMuscleAsync(int id)
    {
        var entity = await _context.TargetMuscles.FindAsync(id);
        if (entity == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Target muscle not found." };

        _context.TargetMuscles.Remove(entity);
        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true, Message = "Target muscle deleted successfully." };
    }

    public async Task<ServiceResponse<bool>> DeleteExerciseAsync(int id)
    {
        var entity = await _context.Exercises.FindAsync(id);
        if (entity == null) return new ServiceResponse<bool> { Success = false, IsNotFound = true, Message = "Exercise not found." };

        _context.Exercises.Remove(entity);
        await _context.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = true, Message = "Exercise deleted successfully." };
    }

    // get

    public async Task<ServiceResponse<List<MuscleGroupResponse>>> GetMuscleGroupsAsync()
    {
        var data = await _context.MuscleGroups
            .Include(m => m.TargetMuscles)
            .Select(m => new MuscleGroupResponse
            {
                Id = m.Id,
                Name = m.Name,
                IconKey = m.IconKey,
                TargetMuscles = m.TargetMuscles.Select(t => new CatalogItemResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    IconKey = t.IconKey
                }).ToList()
            })
            .ToListAsync();

        return new ServiceResponse<List<MuscleGroupResponse>> { Data = data };
    }

    public async Task<ServiceResponse<List<CatalogItemResponse>>> GetExercisesByTargetMuscleAsync(int targetMuscleId)
    {
        var data = await _context.Exercises
            .Where(e => e.TargetMuscleId == targetMuscleId)
            .Select(e => new CatalogItemResponse
            {
                Id = e.Id,
                Name = e.Name,
                IconKey = e.IconKey
            })
            .ToListAsync();

        if (!data.Any()) return new ServiceResponse<List<CatalogItemResponse>> { Success = false, IsNotFound = true, Message = "No exercises found." };

        return new ServiceResponse<List<CatalogItemResponse>> { Data = data };
    }

    public async Task<ServiceResponse<List<ExerciseCatalogResponse>>> GetExercisesByMuscleGroupAsync(int muscleGroupId)
    {
        var exercises = await _context.Exercises
            .Include(e => e.TargetMuscle)
            .Where(e => e.TargetMuscle.MuscleGroupId == muscleGroupId)
            .Select(e => new ExerciseCatalogResponse
            {
                Id = e.Id,
                Name = e.Name,
                IconKey = e.IconKey,
                TargetMuscleName = e.TargetMuscle.Name
            })
            .ToListAsync();

        if (!exercises.Any())
        {
            return new ServiceResponse<List<ExerciseCatalogResponse>>
            {
                Success = false,
                IsNotFound = true,
                Message = "No exercises found for the specified muscle group."
            };
        }

        return new ServiceResponse<List<ExerciseCatalogResponse>>
        {
            Data = exercises,
            Message = "Exercises retrieved successfully."
        };
    }
}