using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;
using PostForge.Infrastructure.Identity.Tenancy;

namespace PostForge.Api.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApplicationTenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                tenantContext.UserId = userId;
                tenantContext.IsSuperUser = string.Equals(
                    context.User.FindFirst("isSuperUser")?.Value,
                    "true",
                    StringComparison.OrdinalIgnoreCase);
            }

            var isAuthEndpoint = context.Request.Path.StartsWithSegments("/api/v1/auth");
            var requestedTenantId = context.Request.Headers["X-Tenant-Id"].ToString();

            if (!isAuthEndpoint)
            {
                if (tenantContext.IsSuperUser)
                {
                    if (Guid.TryParse(requestedTenantId, out var superTenantId))
                        tenantContext.TenantId = superTenantId;
                }
                else
                {
                    if (!Guid.TryParse(requestedTenantId, out var tenantId))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(
                            new { error = "The X-Tenant-Id header is required." });
                        return;
                    }

                    if (!await IsMemberAsync(tenantId, tenantContext.UserId, context))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(
                            new { error = "You are not a member of the requested tenant." });
                        return;
                    }

                    tenantContext.TenantId = tenantId;
                }
            }
        }

        await next(context);
    }

    private static async Task<bool> IsMemberAsync(Guid tenantId, Guid? userId, HttpContext context)
    {
        if (userId is not Guid currentUserId)
            return false;

        var membershipRepository = context.RequestServices.GetRequiredService<ITenantMembershipRepository>();
        return await ((IQueryable<TenantMembership>)membershipRepository)
            .AnyAsync(m => m.TenantId == tenantId && m.UserId == currentUserId);
    }
}