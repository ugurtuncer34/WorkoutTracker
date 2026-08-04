using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutTracker.Dtos;
using WorkoutTracker.Services;

namespace WorkoutTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Workout")]
[Authorize]
public class WorkoutController : ControllerBase
{
    private readonly IWorkoutService _workoutService;

    public WorkoutController(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("sessions")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var response = await _workoutService.StartSessionAsync(request, GetUserId());
        return Ok(response);
    }

    [HttpPost("sessions/from-template")]
    public async Task<IActionResult> StartSessionFromTemplate(
        [FromBody] StartSessionFromTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _workoutService.StartSessionFromTemplateAsync(
            request, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost("logs")]
    public async Task<IActionResult> AddSet([FromBody] AddSetRequest request)
    {
        var response = await _workoutService.AddSetAsync(request, GetUserId());
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        return Ok(response);
    }

    [HttpPut("sessions/{sessionId}/complete")]
    public async Task<IActionResult> CompleteSession(int sessionId)
    {
        var response = await _workoutService.CompleteSessionAsync(sessionId, GetUserId());
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        return Ok(response);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetAllSessions()
    {
        var response = await _workoutService.GetAllSessionsAsync(GetUserId());
        return Ok(response.Data);
    }

    [HttpGet("sessions/{id}")]
    public async Task<IActionResult> GetSessionById(int id)
    {
        var response = await _workoutService.GetSessionByIdAsync(id, GetUserId());
        if (!response.Success) return NotFound(response.Message);
        return Ok(response.Data);
    }

    [HttpGet("sessions/{sessionId}/exercise-plan")]
    public async Task<IActionResult> GetExercisePlan(int sessionId, CancellationToken cancellationToken)
    {
        var response = await _workoutService.GetExercisePlanAsync(
            sessionId, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost("sessions/{sessionId}/exercises")]
    public async Task<IActionResult> AddExercise(
        int sessionId,
        [FromBody] AddWorkoutSessionExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _workoutService.AddExerciseAsync(
            sessionId, request, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPut("sessions/{sessionId}/exercises/{sessionExerciseId}/status")]
    public async Task<IActionResult> UpdateExerciseStatus(
        int sessionId,
        int sessionExerciseId,
        [FromBody] UpdateWorkoutSessionExerciseStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _workoutService.UpdateExerciseStatusAsync(
            sessionId, sessionExerciseId, request, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet("exercises/{exerciseId}/last-performance")]
    public async Task<IActionResult> GetLastPerformance(
        int exerciseId,
        [FromQuery] int? excludeSessionId,
        CancellationToken cancellationToken)
    {
        var response = await _workoutService.GetLastPerformanceAsync(
            exerciseId,
            GetUserId(),
            excludeSessionId,
            cancellationToken);

        if (!response.Success) return NotFound(response.Message);
        return Ok(response);
    }

    [HttpDelete("sessions/{sessionId}/cancel")]
    public async Task<IActionResult> CancelSession(int sessionId)
    {
        var response = await _workoutService.CancelSessionAsync(sessionId, GetUserId());
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        return Ok(response);
    }

    // YENİ GEÇMİŞTEN SİLME ENDPOINT'I
    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> DeleteSession(int sessionId)
    {
        var response = await _workoutService.DeleteSessionAsync(sessionId, GetUserId());
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        return Ok(response);
    }

    [HttpDelete("logs/{id}")]
    public async Task<IActionResult> DeleteSet(int id)
    {
        var response = await _workoutService.DeleteSetAsync(id, GetUserId());
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        return Ok(response);
    }

    private IActionResult ToActionResult<T>(ServiceResponse<T> response)
    {
        if (!response.Success)
        {
            return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        }

        return Ok(response);
    }
}
