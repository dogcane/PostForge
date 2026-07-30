using Mediator;
using PostForge.Application.Ai.DTOs;

namespace PostForge.Application.Ai.Commands.GenerateImage;

public class GenerateImageHandler : IRequestHandler<GenerateImageCommand, ImageResultDto>
{
    public ValueTask<ImageResultDto> Handle(GenerateImageCommand request, CancellationToken cancellationToken)
    {
        var result = new ImageResultDto
        {
            BlobUri = "https://placeholder.blob.core.windows.net/ai-generated/placeholder.png",
            Prompt = request.Prompt
        };

        return ValueTask.FromResult(result);
    }
}
