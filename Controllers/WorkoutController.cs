using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Dtos;
using WorkoutTracker.Services;

namespace WorkoutTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Workout")]
public class WorkoutController : ControllerBase
{
    private readonly IWorkoutService _workoutService;

    public WorkoutController(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var response = await _workoutService.StartSessionAsync(request);
        return Ok(response);
    }

    [HttpPost("logs")]
    public async Task<IActionResult> AddSet([FromBody] AddSetRequest request)
    {
        var response = await _workoutService.AddSetAsync(request);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    [HttpPut("sessions/{sessionId}/complete")]
    public async Task<IActionResult> CompleteSession(int sessionId)
    {
        var response = await _workoutService.CompleteSessionAsync(sessionId);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetAllSessions()
    {
        var response = await _workoutService.GetAllSessionsAsync();
        return Ok(response.Data);
    }

    [HttpGet("sessions/{id}")]
    public async Task<IActionResult> GetSessionById(int id)
    {
        var response = await _workoutService.GetSessionByIdAsync(id);
        if (!response.Success) return NotFound(response.Message);

        return Ok(response.Data);
    }

    [HttpDelete("sessions/{sessionId}/cancel")]
    public async Task<IActionResult> CancelSession(int sessionId)
    {
        var response = await _workoutService.CancelSessionAsync(sessionId);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }
    
    [HttpDelete("logs/{id}")]
    public async Task<IActionResult> DeleteSet(int id)
    {
        var response = await _workoutService.DeleteSetAsync(id);
        if (!response.Success) return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);

        return Ok(response);
    }
}