using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Dtos;
using WorkoutTracker.Services;

namespace WorkoutTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // post

    [HttpPost("muscle-groups")]
    [Tags("Muscle Groups")]
    public async Task<IActionResult> CreateMuscleGroup([FromBody] CreateMuscleGroupRequest request)
    {
        var response = await _catalogService.CreateMuscleGroupAsync(request);
        return Ok(response);
    }

    [HttpPost("target-muscles")]
    [Tags("Target Muscles")]
    public async Task<IActionResult> CreateTargetMuscle([FromBody] CreateTargetMuscleRequest request)
    {
        var response = await _catalogService.CreateTargetMuscleAsync(request);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        
        return Ok(response);
    }

    [HttpPost("exercises")]
    [Tags("Exercises")]
    public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
    {
        var response = await _catalogService.CreateExerciseAsync(request);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    // put

    [HttpPut("muscle-groups/{id}")]
    [Tags("Muscle Groups")]
    public async Task<IActionResult> UpdateMuscleGroup(int id, [FromBody] UpdateMuscleGroupRequest request)
    {
        var response = await _catalogService.UpdateMuscleGroupAsync(id, request);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    [HttpPut("target-muscles/{id}")]
    [Tags("Target Muscles")]
    public async Task<IActionResult> UpdateTargetMuscle(int id, [FromBody] UpdateTargetMuscleRequest request)
    {
        var response = await _catalogService.UpdateTargetMuscleAsync(id, request);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    [HttpPut("exercises/{id}")]
    [Tags("Exercises")]
    public async Task<IActionResult> UpdateExercise(int id, [FromBody] UpdateExerciseRequest request)
    {
        var response = await _catalogService.UpdateExerciseAsync(id, request);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    // delete

    [HttpDelete("muscle-groups/{id}")]
    [Tags("Muscle Groups")]
    public async Task<IActionResult> DeleteMuscleGroup(int id)
    {
        var response = await _catalogService.DeleteMuscleGroupAsync(id);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    [HttpDelete("target-muscles/{id}")]
    [Tags("Target Muscles")]
    public async Task<IActionResult> DeleteTargetMuscle(int id)
    {
        var response = await _catalogService.DeleteTargetMuscleAsync(id);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    [HttpDelete("exercises/{id}")]
    [Tags("Exercises")]
    public async Task<IActionResult> DeleteExercise(int id)
    {
        var response = await _catalogService.DeleteExerciseAsync(id);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    // get

    [HttpGet("muscle-groups")]
    [Tags("Muscle Groups")]
    public async Task<IActionResult> GetMuscleGroups()
    {
        var response = await _catalogService.GetMuscleGroupsAsync();
        return Ok(response.Data);
    }

    [HttpGet("target-muscles/{targetMuscleId}/exercises")]
    [Tags("Exercises")]
    public async Task<IActionResult> GetExercisesByTargetMuscle(int targetMuscleId)
    {
        var response = await _catalogService.GetExercisesByTargetMuscleAsync(targetMuscleId);
        if (!response.Success) return NotFound(response.Message);

        return Ok(response.Data);
    }

    [HttpGet("muscle-groups/{id}/exercises")]
    public async Task<IActionResult> GetExercisesByMuscleGroup(int id)
    {
        var response = await _catalogService.GetExercisesByMuscleGroupAsync(id);
        
        if (!response.Success) 
        {
            return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        }

        return Ok(response.Data);
    }
}