using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Data;
using WorkoutTracker.Dtos;
using WorkoutTracker.Entities;

namespace WorkoutTracker.Services;

public class WorkoutTemplateService : IWorkoutTemplateService
{
    private const int TemplateNameMaxLength = 100;
    private const int TemplateNotesMaxLength = 1000;
    private const int ExerciseNotesMaxLength = 500;
    private readonly AppDbContext _context;

    public WorkoutTemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<List<WorkoutTemplateListResponse>>> GetAllAsync(
        int userId,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var templates = await _context.WorkoutTemplates
            .AsNoTracking()
            .Where(t => t.UserId == userId && (includeArchived || !t.IsArchived))
            .Include(t => t.Exercises)
                .ThenInclude(te => te.Exercise)
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return new ServiceResponse<List<WorkoutTemplateListResponse>>
        {
            Data = templates.Select(MapListResponse).ToList()
        };
    }

    public async Task<ServiceResponse<WorkoutTemplateResponse>> GetByIdAsync(
        int id,
        int userId,
        CancellationToken cancellationToken)
    {
        var response = await LoadResponseAsync(id, userId, cancellationToken);
        return response is null
            ? NotFound()
            : new ServiceResponse<WorkoutTemplateResponse> { Data = response };
    }

    public async Task<ServiceResponse<WorkoutTemplateResponse>> CreateAsync(
        WorkoutTemplateRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(request, cancellationToken);
        if (!validation.Success)
        {
            return ValidationFailure(validation.Message);
        }

        var now = DateTime.UtcNow;
        var template = new WorkoutTemplate
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Category = request.Category,
            Notes = NormalizeNotes(request.Notes),
            IsArchived = false,
            CreatedAt = now,
            UpdatedAt = now,
            Exercises = BuildExercises(request.Exercises, now)
        };

        _context.WorkoutTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        var response = await LoadResponseAsync(template.Id, userId, cancellationToken);
        return new ServiceResponse<WorkoutTemplateResponse>
        {
            Data = response!,
            Message = "Workout template created successfully."
        };
    }

    public async Task<ServiceResponse<WorkoutTemplateResponse>> UpdateAsync(
        int id,
        WorkoutTemplateRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        var template = await _context.WorkoutTemplates
            .Include(t => t.Exercises)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

        if (template is null)
        {
            return NotFound();
        }

        var validation = await ValidateAsync(request, cancellationToken);
        if (!validation.Success)
        {
            return ValidationFailure(validation.Message);
        }

        var now = DateTime.UtcNow;
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        template.Name = request.Name.Trim();
        template.Category = request.Category;
        template.Notes = NormalizeNotes(request.Notes);
        template.UpdatedAt = now;

        _context.WorkoutTemplateExercises.RemoveRange(template.Exercises);
        await _context.SaveChangesAsync(cancellationToken);

        var replacements = BuildExercises(request.Exercises, now);
        foreach (var replacement in replacements)
        {
            replacement.WorkoutTemplateId = template.Id;
        }

        _context.WorkoutTemplateExercises.AddRange(replacements);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var response = await LoadResponseAsync(template.Id, userId, cancellationToken);
        return new ServiceResponse<WorkoutTemplateResponse>
        {
            Data = response!,
            Message = "Workout template updated successfully."
        };
    }

    public async Task<ServiceResponse<WorkoutTemplateResponse>> SetArchivedAsync(
        int id,
        bool isArchived,
        int userId,
        CancellationToken cancellationToken)
    {
        var template = await _context.WorkoutTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

        if (template is null)
        {
            return NotFound();
        }

        template.IsArchived = isArchived;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var response = await LoadResponseAsync(template.Id, userId, cancellationToken);
        return new ServiceResponse<WorkoutTemplateResponse>
        {
            Data = response!,
            Message = isArchived
                ? "Workout template archived successfully."
                : "Workout template unarchived successfully."
        };
    }

    public async Task<ServiceResponse<WorkoutTemplateResponse>> CloneAsync(
        int id,
        CloneWorkoutTemplateRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        var source = await _context.WorkoutTemplates
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == userId)
            .Include(t => t.Exercises)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return NotFound();
        }

        string cloneName;
        if (request.Name is not null)
        {
            cloneName = request.Name.Trim();
            if (cloneName.Length == 0)
            {
                return ValidationFailure("Name must not be empty.");
            }

            if (cloneName.Length > TemplateNameMaxLength)
            {
                return ValidationFailure($"Name must not exceed {TemplateNameMaxLength} characters.");
            }
        }
        else
        {
            cloneName = source.Name.Length <= TemplateNameMaxLength - 5
                ? $"{source.Name} Copy"
                : $"{source.Name[..(TemplateNameMaxLength - 5)]} Copy";
        }

        var now = DateTime.UtcNow;
        var clone = new WorkoutTemplate
        {
            UserId = userId,
            Name = cloneName,
            Category = source.Category,
            Notes = source.Notes,
            IsArchived = false,
            CreatedAt = now,
            UpdatedAt = now,
            Exercises = source.Exercises
                .OrderBy(e => e.Position)
                .ThenBy(e => e.Id)
                .Select(e => new WorkoutTemplateExercise
                {
                    ExerciseId = e.ExerciseId,
                    Position = e.Position,
                    TargetSetCount = e.TargetSetCount,
                    RepMin = e.RepMin,
                    RepMax = e.RepMax,
                    TargetDurationSeconds = e.TargetDurationSeconds,
                    SuggestedWeightKg = e.SuggestedWeightKg,
                    Notes = e.Notes,
                    IsOptional = e.IsOptional,
                    CreatedAt = now
                })
                .ToList()
        };

        _context.WorkoutTemplates.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        var response = await LoadResponseAsync(clone.Id, userId, cancellationToken);
        return new ServiceResponse<WorkoutTemplateResponse>
        {
            Data = response!,
            Message = "Workout template cloned successfully."
        };
    }

    private async Task<(bool Success, string Message)> ValidateAsync(
        WorkoutTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var trimmedName = request.Name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0)
        {
            errors.Add("Name is required.");
        }
        else if (trimmedName.Length > TemplateNameMaxLength)
        {
            errors.Add($"Name must not exceed {TemplateNameMaxLength} characters.");
        }

        if (request.Notes?.Trim().Length > TemplateNotesMaxLength)
        {
            errors.Add($"Notes must not exceed {TemplateNotesMaxLength} characters.");
        }

        if (!Enum.IsDefined(request.Category))
        {
            errors.Add("Category is invalid.");
        }

        if (request.Exercises is null || request.Exercises.Count == 0)
        {
            errors.Add("At least one exercise is required.");
            return (false, string.Join(" ", errors));
        }

        var duplicateExerciseIds = request.Exercises
            .GroupBy(e => e.ExerciseId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .OrderBy(id => id)
            .ToList();
        if (duplicateExerciseIds.Count > 0)
        {
            errors.Add($"Duplicate ExerciseId values are not allowed: {string.Join(", ", duplicateExerciseIds)}.");
        }

        var duplicatePositions = request.Exercises
            .GroupBy(e => e.Position)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .OrderBy(position => position)
            .ToList();
        if (duplicatePositions.Count > 0)
        {
            errors.Add($"Duplicate positions are not allowed: {string.Join(", ", duplicatePositions)}.");
        }

        for (var index = 0; index < request.Exercises.Count; index++)
        {
            var item = request.Exercises[index];
            var label = $"Exercise at index {index}";
            if (item.ExerciseId <= 0) errors.Add($"{label}: ExerciseId must be greater than zero.");
            if (item.Position <= 0) errors.Add($"{label}: Position must be greater than zero.");
            if (item.TargetSetCount <= 0) errors.Add($"{label}: TargetSetCount must be greater than zero.");
            if (item.RepMin is <= 0) errors.Add($"{label}: RepMin must be positive when provided.");
            if (item.RepMax is <= 0) errors.Add($"{label}: RepMax must be positive when provided.");
            if (item.RepMin.HasValue && item.RepMax.HasValue && item.RepMin > item.RepMax)
                errors.Add($"{label}: RepMin must not exceed RepMax.");
            if (item.TargetDurationSeconds is <= 0)
                errors.Add($"{label}: TargetDurationSeconds must be positive when provided.");
            if (item.SuggestedWeightKg < 0)
                errors.Add($"{label}: SuggestedWeightKg must not be negative.");
            if (item.Notes?.Trim().Length > ExerciseNotesMaxLength)
                errors.Add($"{label}: Notes must not exceed {ExerciseNotesMaxLength} characters.");
        }

        var requestedIds = request.Exercises
            .Select(e => e.ExerciseId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        var exerciseTypes = await _context.Exercises
            .AsNoTracking()
            .Where(e => requestedIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Type })
            .ToDictionaryAsync(e => e.Id, e => e.Type, cancellationToken);

        var missingIds = requestedIds.Where(id => !exerciseTypes.ContainsKey(id)).OrderBy(id => id).ToList();
        if (missingIds.Count > 0)
        {
            errors.Add($"Exercises were not found: {string.Join(", ", missingIds)}.");
        }

        for (var index = 0; index < request.Exercises.Count; index++)
        {
            var item = request.Exercises[index];
            if (!exerciseTypes.TryGetValue(item.ExerciseId, out var exerciseType)) continue;

            var label = $"Exercise at index {index} ({exerciseType})";
            switch (exerciseType)
            {
                case ExerciseType.RepsAndWeight:
                case ExerciseType.RepsWithOptionalWeight:
                    if (item.TargetDurationSeconds.HasValue)
                        errors.Add($"{label}: TargetDurationSeconds must be null.");
                    break;
                case ExerciseType.RepsOnly:
                    if (item.SuggestedWeightKg.HasValue)
                        errors.Add($"{label}: SuggestedWeightKg must be null.");
                    if (item.TargetDurationSeconds.HasValue)
                        errors.Add($"{label}: TargetDurationSeconds must be null.");
                    break;
                case ExerciseType.DurationOnly:
                    if (item.RepMin.HasValue || item.RepMax.HasValue)
                        errors.Add($"{label}: RepMin and RepMax must be null.");
                    if (item.SuggestedWeightKg.HasValue)
                        errors.Add($"{label}: SuggestedWeightKg must be null.");
                    break;
            }
        }

        return (errors.Count == 0, string.Join(" ", errors));
    }

    private static List<WorkoutTemplateExercise> BuildExercises(
        IEnumerable<WorkoutTemplateExerciseRequest> requests,
        DateTime createdAt)
    {
        return requests
            .OrderBy(e => e.Position)
            .Select((e, index) => new WorkoutTemplateExercise
            {
                ExerciseId = e.ExerciseId,
                Position = index + 1,
                TargetSetCount = e.TargetSetCount,
                RepMin = e.RepMin,
                RepMax = e.RepMax,
                TargetDurationSeconds = e.TargetDurationSeconds,
                SuggestedWeightKg = e.SuggestedWeightKg,
                Notes = NormalizeNotes(e.Notes),
                IsOptional = e.IsOptional,
                CreatedAt = createdAt
            })
            .ToList();
    }

    private async Task<WorkoutTemplateResponse?> LoadResponseAsync(
        int id,
        int userId,
        CancellationToken cancellationToken)
    {
        var template = await _context.WorkoutTemplates
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == userId)
            .Include(t => t.Exercises)
                .ThenInclude(te => te.Exercise)
                    .ThenInclude(e => e.TargetMuscle)
                        .ThenInclude(tm => tm.MuscleGroup)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        return template is null ? null : MapDetailResponse(template);
    }

    private static WorkoutTemplateListResponse MapListResponse(WorkoutTemplate template)
    {
        return new WorkoutTemplateListResponse
        {
            Id = template.Id,
            Name = template.Name,
            Category = template.Category.ToString(),
            Notes = template.Notes,
            IsArchived = template.IsArchived,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Exercises = template.Exercises
                .OrderBy(e => e.Position)
                .ThenBy(e => e.Id)
                .Select(e => new WorkoutTemplateExerciseSummaryResponse
                {
                    TemplateExerciseId = e.Id,
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.Exercise.Name,
                    ExerciseType = e.Exercise.Type.ToString(),
                    IconKey = e.Exercise.IconKey,
                    Position = e.Position
                })
                .ToList()
        };
    }

    private static WorkoutTemplateResponse MapDetailResponse(WorkoutTemplate template)
    {
        return new WorkoutTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Category = template.Category.ToString(),
            Notes = template.Notes,
            IsArchived = template.IsArchived,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Exercises = template.Exercises
                .OrderBy(e => e.Position)
                .ThenBy(e => e.Id)
                .Select(e => new WorkoutTemplateExerciseResponse
                {
                    TemplateExerciseId = e.Id,
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.Exercise.Name,
                    ExerciseType = e.Exercise.Type.ToString(),
                    IconKey = e.Exercise.IconKey,
                    TargetMuscleId = e.Exercise.TargetMuscleId,
                    TargetMuscleName = e.Exercise.TargetMuscle.Name,
                    MuscleGroupId = e.Exercise.TargetMuscle.MuscleGroupId,
                    MuscleGroupName = e.Exercise.TargetMuscle.MuscleGroup.Name,
                    Position = e.Position,
                    TargetSetCount = e.TargetSetCount,
                    RepMin = e.RepMin,
                    RepMax = e.RepMax,
                    TargetDurationSeconds = e.TargetDurationSeconds,
                    SuggestedWeightKg = e.SuggestedWeightKg,
                    Notes = e.Notes,
                    IsOptional = e.IsOptional
                })
                .ToList()
        };
    }

    private static string? NormalizeNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    private static ServiceResponse<WorkoutTemplateResponse> ValidationFailure(string message)
    {
        return new ServiceResponse<WorkoutTemplateResponse> { Success = false, Message = message };
    }

    private static ServiceResponse<WorkoutTemplateResponse> NotFound()
    {
        return new ServiceResponse<WorkoutTemplateResponse>
        {
            Success = false,
            IsNotFound = true,
            Message = "Workout template not found."
        };
    }
}
