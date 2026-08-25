using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.Ai.Commands.GenerateCaption;
using PostForge.Application.Ai.Commands.GenerateImage;
using PostForge.Application.Ai.DTOs;

namespace PostForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/ai")]
public class AiController(IMediator mediator) : ControllerBase
{

    [HttpPost("caption")]
    public async Task<ActionResult<CaptionResultDto>> GenerateCaption([FromBody] GenerateCaptionCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("image")]
    public async Task<ActionResult<ImageResultDto>> GenerateImage([FromBody] GenerateImageCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }
}
