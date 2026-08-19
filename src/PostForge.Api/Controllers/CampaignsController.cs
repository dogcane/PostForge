using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.Campaigns.Commands.CreateCampaign;
using PostForge.Application.Campaigns.Commands.DeleteCampaign;
using PostForge.Application.Campaigns.Commands.UpdateCampaign;
using PostForge.Application.Campaigns.DTOs;
using PostForge.Application.Campaigns.Queries.GetAllCampaigns;
using PostForge.Application.Campaigns.Queries.GetCampaignById;
using PostForge.Domain.ValueObjects;

namespace PostForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CampaignsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CampaignDto>>> GetAll(
        [FromQuery] CampaignGoal? goal,
        [FromQuery] CampaignChannel? channel,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = new GetAllCampaignsQuery(goal, channel, dateFrom, dateTo);
        var campaigns = await _mediator.Send(query);
        return Ok(campaigns);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampaignDto>> GetById(Guid id)
    {
        var query = new GetCampaignByIdQuery(id);
        var campaign = await _mediator.Send(query);

        if (campaign is null)
            return NotFound();

        return Ok(campaign);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCampaignCommand command)
    {
        var campaignId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = campaignId }, campaignId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCampaignCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id mismatch between route and body.");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCampaignCommand(id));
        return NoContent();
    }
}
