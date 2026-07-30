namespace PostForge.Application.Ai.DTOs;

public class ImageResultDto
{
    public string BlobUri { get; set; } = null!;
    public string? Prompt { get; set; }
}
