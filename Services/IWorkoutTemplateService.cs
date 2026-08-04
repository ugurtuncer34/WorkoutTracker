using WorkoutTracker.Dtos;

namespace WorkoutTracker.Services;

public interface IWorkoutTemplateService
{
    Task<ServiceResponse<List<WorkoutTemplateListResponse>>> GetAllAsync(int userId, bool includeArchived, CancellationToken cancellationToken);
    Task<ServiceResponse<WorkoutTemplateResponse>> GetByIdAsync(int id, int userId, CancellationToken cancellationToken);
    Task<ServiceResponse<WorkoutTemplateResponse>> CreateAsync(WorkoutTemplateRequest request, int userId, CancellationToken cancellationToken);
    Task<ServiceResponse<WorkoutTemplateResponse>> UpdateAsync(int id, WorkoutTemplateRequest request, int userId, CancellationToken cancellationToken);
    Task<ServiceResponse<WorkoutTemplateResponse>> SetArchivedAsync(int id, bool isArchived, int userId, CancellationToken cancellationToken);
    Task<ServiceResponse<WorkoutTemplateResponse>> CloneAsync(int id, CloneWorkoutTemplateRequest request, int userId, CancellationToken cancellationToken);
}
