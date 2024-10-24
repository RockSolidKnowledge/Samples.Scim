using System.Linq.Expressions;
using FluentAssertions;
using Rsk.AspNetCore.Scim.Constants;
using SimpleApp.Services;

namespace Tests;

public class ScimPatchMapBuilderTests
{
    private string schema = ScimSchemas.User;

    [Fact]
    public void Map_WhenCalled_ShouldReturnScimPatchMapBuilder()
    {
        ScimPatchMapBuilder<AppUser> sut = CreateSut;

        string attribute = "name";
        Expression<Func<AppUser, string>> propertyAccessor = user => user.Username;

        var result = sut.Map(attribute, propertyAccessor);

        result.Should().BeOfType<ScimPatchMapBuilder<AppUser>>();
    }

    private ScimPatchMapBuilder<AppUser> CreateSut => new(schema);
}