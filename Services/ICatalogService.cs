using WorkoutTracker.Dtos;

namespace WorkoutTracker.Services;

public interface ICatalogService
{
    Task<ServiceResponse<CatalogItemResponse>> CreateMuscleGroupAsync(CreateMuscleGroupRequest request);
    Task<ServiceResponse<CatalogItemResponse>> CreateTargetMuscleAsync(CreateTargetMuscleRequest request);
    Task<ServiceResponse<CatalogItemResponse>> CreateExerciseAsync(CreateExerciseRequest request);

    Task<ServiceResponse<bool>> UpdateMuscleGroupAsync(int id, UpdateMuscleGroupRequest request);
    Task<ServiceResponse<bool>> UpdateTargetMuscleAsync(int id, UpdateTargetMuscleRequest request);
    Task<ServiceResponse<bool>> UpdateExerciseAsync(int id, UpdateExerciseRequest request);

    Task<ServiceResponse<bool>> DeleteMuscleGroupAsync(int id);
    Task<ServiceResponse<bool>> DeleteTargetMuscleAsync(int id);
    Task<ServiceResponse<bool>> DeleteExerciseAsync(int id);

    Task<ServiceResponse<List<MuscleGroupResponse>>> GetMuscleGroupsAsync();
    Task<ServiceResponse<List<CatalogItemResponse>>> GetExercisesByTargetMuscleAsync(int targetMuscleId);
    Task<ServiceResponse<List<ExerciseCatalogResponse>>> GetExercisesByMuscleGroupAsync(int muscleGroupId);
}