using Mediator;
using PostForge.Application.Ai.DTOs;

namespace PostForge.Application.Ai.Commands.GenerateCaption;

public class GenerateCaptionHandler : IRequestHandler<GenerateCaptionCommand, CaptionResultDto>
{
    public ValueTask<CaptionResultDto> Handle(GenerateCaptionCommand request, CancellationToken cancellationToken)
    {
        var caption = $"[AI caption stub for brief: {request.Brief}]";

        var result = new CaptionResultDto
        {
            Caption = caption
        };

        return ValueTask.FromResult(result);
    }
}
