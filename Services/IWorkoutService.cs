using WorkoutTracker.Dtos;

namespace WorkoutTracker.Services;

public interface IWorkoutService
{
    Task<ServiceResponse<StartSessionResponse>> StartSessionAsync(StartSessionRequest request, int userId);
    Task<ServiceResponse<AddSetResponse>> AddSetAsync(AddSetRequest request, int userId);
    Task<ServiceResponse<bool>> CompleteSessionAsync(int sessionId, int userId);
    Task<ServiceResponse<bool>> CancelSessionAsync(int sessionId, int userId);
    Task<ServiceResponse<bool>> DeleteSessionAsync(int sessionId, int userId);
    Task<ServiceResponse<List<WorkoutSessionResponse>>> GetAllSessionsAsync(int userId);
    Task<ServiceResponse<WorkoutSessionResponse>> GetSessionByIdAsync(int id, int userId);
    Task<ServiceResponse<bool>> DeleteSetAsync(int logId, int userId);
}