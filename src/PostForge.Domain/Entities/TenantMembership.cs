using ECO;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class TenantMembership : AggregateRoot<Guid>
{
    #region Fields
    #endregion

    #region Properties
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    #endregion

    #region ctor
    private TenantMembership() : base(Guid.NewGuid())
    {
    }

    private TenantMembership(Guid tenantId, Guid userId) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        UserId = userId;
        JoinedAtUtc = DateTime.UtcNow;
    }
    #endregion

    #region Methods
    protected static OperationResult Validate(Guid tenantId, Guid userId)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(userId, "UserId").Condition(v => v != Guid.Empty);
        return result;
    }

    public static OperationResult<TenantMembership> Create(Guid tenantId, Guid userId)
        => Validate(tenantId, userId)
            .IfSuccessThenReturn<TenantMembership>(() => new TenantMembership(tenantId, userId));
    #endregion
}
