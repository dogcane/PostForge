using Mediator;
using PostForge.Application.Auth.DTOs;

namespace PostForge.Application.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery() : IRequest<CurrentUserDto?>;