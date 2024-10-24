using FluentAssertions;
using Moq;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Parsers;
using SimpleApp.Services;

namespace Tests;

public class ScimPatchMapTests
{
    private Dictionary<string, IScimPatchOperationExecutor<AppUser>> map;
    public ScimPatchMapTests()
    {
        map = new Dictionary<string, IScimPatchOperationExecutor<AppUser>>();
    }

    [Fact]
    public void TryGetPatchExecutor_WhenCalledAndPathExpressionIsNull_ShouldThrowArgumentNullException()
    {
        var sut = CreateSut;

        Action act = () => sut.TryGetPatchExecutor(null, out _);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryGetPatchExecutor_WhenCalledAndPathIsNotInMap_ShouldReturnFalse()
    {
        var sut = CreateSut;

        PathExpression path = new();

        bool result = sut.TryGetPatchExecutor(path, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryGetPatchExecutor_WhenCalledAndPathIsInMap_ShouldReturnTrue()
    {
        var sut = CreateSut;

        PathExpression path = new PathExpression(new AttributePathExpression(ScimSchemas.User, "userName"));

        IScimPatchOperationExecutor<AppUser> opEx = new Mock<IScimPatchOperationExecutor<AppUser>>().Object;

        map.Add($"{ScimSchemas.User}:userName", opEx);

        bool result = sut.TryGetPatchExecutor(path, out opEx);

        result.Should().BeTrue();
    }

    private ScimPatchMap<AppUser> CreateSut => new(map);
}