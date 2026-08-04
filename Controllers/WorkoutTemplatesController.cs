using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutTracker.Dtos;
using WorkoutTracker.Services;

namespace WorkoutTracker.Controllers;

[ApiController]
[Route("api/workout-templates")]
[Tags("Workout Templates")]
[Authorize]
public class WorkoutTemplatesController : ControllerBase
{
    private readonly IWorkoutTemplateService _workoutTemplateService;

    public WorkoutTemplatesController(IWorkoutTemplateService workoutTemplateService)
    {
        _workoutTemplateService = workoutTemplateService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var response = await _workoutTemplateService.GetAllAsync(
            GetUserId(), includeArchived, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _workoutTemplateService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] WorkoutTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _workoutTemplateService.CreateAsync(request, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] WorkoutTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _workoutTemplateService.UpdateAsync(id, request, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPut("{id}/archive")]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        var response = await _workoutTemplateService.SetArchivedAsync(
            id, true, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPut("{id}/unarchive")]
    public async Task<IActionResult> Unarchive(int id, CancellationToken cancellationToken)
    {
        var response = await _workoutTemplateService.SetArchivedAsync(
            id, false, GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost("{id}/clone")]
    public async Task<IActionResult> Clone(
        int id,
        [FromBody] CloneWorkoutTemplateRequest? request,
        CancellationToken cancellationToken)
    {
        var response = await _workoutTemplateService.CloneAsync(
            id, request ?? new CloneWorkoutTemplateRequest(), GetUserId(), cancellationToken);
        return ToActionResult(response);
    }

    private IActionResult ToActionResult(ServiceResponse<WorkoutTemplateResponse> response)
    {
        if (!response.Success)
        {
            return response.IsNotFound ? NotFound(response.Message) : BadRequest(response.Message);
        }

        return Ok(response);
    }
}
