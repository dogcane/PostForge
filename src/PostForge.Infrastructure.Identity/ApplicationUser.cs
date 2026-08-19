using Microsoft.AspNetCore.Identity;

namespace PostForge.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsSuperUser { get; set; }
}