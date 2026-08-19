using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.Scheduling.Commands.MarkSlotFailed;
using PostForge.Application.Scheduling.Commands.MarkSlotPublished;
using PostForge.Application.Scheduling.Commands.SchedulePost;
using PostForge.Application.Scheduling.DTOs;
using PostForge.Application.Scheduling.Queries.GetPendingSlots;
using PostForge.Application.Scheduling.Queries.GetSlotsByPostId;

namespace PostForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/scheduling")]
public class SchedulingController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("schedule")]
    public async Task<ActionResult<Guid>> Schedule([FromBody] SchedulePostCommand command)
    {
        var slotId = await _mediator.Send(command);
        return CreatedAtAction(null, null, new { slotId }, slotId);
    }

    [HttpPost("{slotId:guid}/publish")]
    public async Task<ActionResult> MarkPublished(Guid slotId)
    {
        await _mediator.Send(new MarkSlotPublishedCommand(slotId));
        return NoContent();
    }

    [HttpPost("{slotId:guid}/fail")]
    public async Task<ActionResult> MarkFailed(Guid slotId, [FromBody] MarkSlotFailedCommand command)
    {
        if (slotId != command.SlotId)
            return BadRequest("Id mismatch between route and body.");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<ScheduleSlotDto>>> GetPending()
    {
        var slots = await _mediator.Send(new GetPendingSlotsQuery());
        return Ok(slots);
    }

    [HttpGet("by-post/{postId:guid}")]
    public async Task<ActionResult<List<ScheduleSlotDto>>> GetByPostId(Guid postId)
    {
        var slots = await _mediator.Send(new GetSlotsByPostIdQuery(postId));
        return Ok(slots);
    }
}
