using Mediator;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.Ai.Commands.GenerateCaption;
using PostForge.Application.Ai.Commands.GenerateImage;
using PostForge.Application.Ai.DTOs;

namespace PostForge.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("caption")]
    public async Task<ActionResult<CaptionResultDto>> GenerateCaption([FromBody] GenerateCaptionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("image")]
    public async Task<ActionResult<ImageResultDto>> GenerateImage([FromBody] GenerateImageCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
