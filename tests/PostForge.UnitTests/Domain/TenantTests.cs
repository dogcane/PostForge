using FluentAssertions;
using PostForge.Domain.Entities;

namespace PostForge.UnitTests.Domain;

public class TenantTests
{
    [Fact]
    public void CreatingTenant_ShouldSetProperties()
    {
        var result = Tenant.Create("Acme Corp", "acme");

        result.Success.Should().BeTrue();
        result.Value!.Name.Should().Be("Acme Corp");
        result.Value.Slug.Should().Be("acme");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreatingTenant_WithEmptyName_ShouldReturnFailure()
    {
        var result = Tenant.Create("", "acme");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Name");
    }

    [Fact]
    public void CreatingTenant_WithInvalidSlug_ShouldReturnFailure()
    {
        var result = Tenant.Create("Acme Corp", "Invalid Slug!");

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "Slug");
    }

    [Fact]
    public void UpdatingDetails_ShouldChangeName()
    {
        var tenant = Tenant.Create("Acme Corp", "acme").Value!;

        var result = tenant.UpdateDetails("Acme Inc", "acme");

        result.Success.Should().BeTrue();
        tenant.Name.Should().Be("Acme Inc");
        tenant.Slug.Should().Be("acme");
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var tenant = Tenant.Create("Acme Corp", "acme").Value!;

        var result = tenant.Deactivate();

        result.Success.Should().BeTrue();
        tenant.IsActive.Should().BeFalse();
    }
}