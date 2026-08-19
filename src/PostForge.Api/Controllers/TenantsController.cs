using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.Tenants.Commands.AddUserToTenant;
using PostForge.Application.Tenants.Commands.CreateTenant;
using PostForge.Application.Tenants.Commands.RemoveUserFromTenant;
using PostForge.Application.Tenants.DTOs;
using PostForge.Application.Tenants.Queries.GetAllTenants;
using PostForge.Application.Tenants.Queries.GetTenantById;
using PostForge.Application.Tenants.Queries.GetTenantUsers;
using PostForge.Infrastructure.Identity;

namespace PostForge.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
[Authorize(Policy = PostForge.Infrastructure.Identity.DependencyInjection.SuperAdminPolicy)]
public class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TenantDto>>> GetAll()
        => Ok(await mediator.Send(new GetAllTenantsQuery()));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id)
    {
        var tenant = await mediator.Send(new GetTenantByIdQuery(id));
        if (tenant is null)
            return NotFound();

        return Ok(tenant);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTenantCommand command)
    {
        var tenantId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = tenantId }, tenantId);
    }

    [HttpGet("{id:guid}/users")]
    public async Task<ActionResult<List<TenantUserDto>>> GetUsers(Guid id)
        => Ok(await mediator.Send(new GetTenantUsersQuery(id)));

    [HttpPost("{id:guid}/users")]
    public async Task<ActionResult<Guid>> AddUser(Guid id, [FromBody] AddUserToTenantCommand command)
    {
        if (id != command.TenantId)
            return BadRequest("Tenant id mismatch between route and body.");

        var userId = await mediator.Send(command);
        return Ok(userId);
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    public async Task<ActionResult> RemoveUser(Guid id, Guid userId)
    {
        await mediator.Send(new RemoveUserFromTenantCommand(id, userId));
        return NoContent();
    }
}