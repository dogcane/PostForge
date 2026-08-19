using ECO;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class TenantMembership : AggregateRoot<Guid>
{
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    private TenantMembership() : base(Guid.NewGuid())
    {
    }

    private TenantMembership(Guid tenantId, Guid userId) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        UserId = userId;
        JoinedAtUtc = DateTime.UtcNow;
    }

    public static OperationResult<TenantMembership> Create(Guid tenantId, Guid userId)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(userId, "UserId").Condition(v => v != Guid.Empty);
        if (!result.Success)
            return result;
        return OperationResult<TenantMembership>.MakeSuccess(new TenantMembership(tenantId, userId));
    }
}