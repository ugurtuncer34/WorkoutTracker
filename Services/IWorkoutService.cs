using WorkoutTracker.Dtos;

namespace WorkoutTracker.Services;

public interface IWorkoutService
{
    Task<ServiceResponse<StartSessionResponse>> StartSessionAsync(StartSessionRequest request);
    Task<ServiceResponse<AddSetResponse>> AddSetAsync(AddSetRequest request);
    Task<ServiceResponse<bool>> CompleteSessionAsync(int sessionId);
    Task<ServiceResponse<bool>> CancelSessionAsync(int sessionId);
    Task<ServiceResponse<List<WorkoutSessionResponse>>> GetAllSessionsAsync();
    Task<ServiceResponse<WorkoutSessionResponse>> GetSessionByIdAsync(int id);
    Task<ServiceResponse<bool>> DeleteSetAsync(int logId);
}