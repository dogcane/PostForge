using ECO;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class Tenant : AggregateRoot<Guid>
{
    public Guid Id => Identity;
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

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

    public static OperationResult<Tenant> Create(string name, string slug)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(name, "Name").Required().StringLength(200)
            .With(slug, "Slug").Required().StringLength(100).StringMatch("^[a-z0-9-]+$");
        if (!result.Success)
            return result;
        return OperationResult<Tenant>.MakeSuccess(new Tenant(name, slug));
    }

    public OperationResult UpdateDetails(string name, string slug)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(name, "Name").Required().StringLength(200)
            .With(slug, "Slug").Required().StringLength(100).StringMatch("^[a-z0-9-]+$");
        if (!result.Success)
            return result;
        Name = name;
        Slug = slug;
        return OperationResult.MakeSuccess();
    }

    public OperationResult Deactivate()
    {
        if (!IsActive)
            return OperationResult.MakeFailure(ErrorMessage.Create("IsActive", "Tenant is already inactive."));
        IsActive = false;
        return OperationResult.MakeSuccess();
    }
}