using WorkoutTracker.Dtos;

namespace WorkoutTracker.Services;

public interface IAuthService
{
    Task<ServiceResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResponse<AuthResponse>> LoginAsync(LoginRequest request);
}