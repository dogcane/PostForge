using ECO;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class Tenant : AggregateRoot<Guid>
{
    #region Fields
    #endregion

    #region Properties
    public Guid Id => Identity;
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsActive { get; private set; }
    #endregion

    #region ctor
    private Tenant() : base(Guid.NewGuid())
    {
        Name = null!;
        Slug = null!;
    }

    private Tenant(string name, string slug) : base(Guid.NewGuid())
    {
        Name = name;
        Slug = slug;
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }
    #endregion

    #region Methods
    protected static OperationResult Validate(string name, string slug)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(name, "Name").Required().StringLength(200)
            .With(slug, "Slug").Required().StringLength(100).StringMatch("^[a-z0-9-]+$");
        return result;
    }

    public static OperationResult<Tenant> Create(string name, string slug)
        => Validate(name, slug)
            .IfSuccessThenReturn<Tenant>(() => new Tenant(name, slug));

    public OperationResult UpdateDetails(string name, string slug)
        => Validate(name, slug)
            .IfSuccess(_ =>
            {
                Name = name;
                Slug = slug;
            });

    public OperationResult Deactivate()
        => !IsActive
            ? OperationResult.MakeFailure(ErrorMessage.Create("IsActive", "Tenant is already inactive."))
            : OperationResult.MakeSuccess().IfSuccess(_ => IsActive = false);
    #endregion
}
