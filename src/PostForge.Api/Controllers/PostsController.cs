using Mediator;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.Posts.Commands.ChangePostStatus;
using PostForge.Application.Posts.Commands.CreatePost;
using PostForge.Application.Posts.Commands.DeletePost;
using PostForge.Application.Posts.Commands.UpdatePost;
using PostForge.Application.Posts.DTOs;
using PostForge.Application.Posts.Queries.GetAllPosts;
using PostForge.Application.Posts.Queries.GetPostById;
using PostForge.Domain.ValueObjects;

namespace PostForge.Api.Controllers;

[ApiController]
[Route("api/v1/posts")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<PostDto>>> GetAll(
        [FromQuery] PostStatus? status,
        [FromQuery] string? platform,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = new GetAllPostsQuery(status, platform, dateFrom, dateTo);
        var posts = await _mediator.Send(query);
        return Ok(posts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetById(Guid id)
    {
        var query = new GetPostByIdQuery(id);
        var post = await _mediator.Send(query);

        if (post is null)
            return NotFound();

        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePostCommand command)
    {
        var postId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = postId }, postId);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePostCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id mismatch between route and body.");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePostCommand(id));
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> ChangeStatus(Guid id, [FromBody] ChangePostStatusCommand command)
    {
        if (id != command.PostId)
            return BadRequest("Id mismatch between route and body.");

        await _mediator.Send(command);
        return NoContent();
    }
}
