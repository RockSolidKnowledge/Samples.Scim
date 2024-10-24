using FluentAssertions;
using Moq;
using Rsk.AspNetCore.Scim.Parsers;
using Rsk.AspNetCore.Scim.Stores;
using SimpleApp.SCIM;
using SimpleApp.Services;

namespace Tests;

public class ScimPatcherTests
{
    private readonly Mock<IScimPatchMap<AppUser>> patchMap;

    public ScimPatcherTests()
    {
        patchMap = new Mock<IScimPatchMap<AppUser>>();
    }

    [Fact]
    public void TryPatch_WhenCalledAndPatchMapsDontContainMapForPath_ShouldReturnFalse()
    {
        ScimPatcher<AppUser> sut = CreateSut;

        bool result = sut.TryPatch(new AppUser(), new PatchCommand(PatchOperation.Add, PathExpression.Root));

        result.Should().BeFalse();
    }

    [Fact]
    public void TryPatch_WhenCalledAndAPatchMapContainsKeyForPath_ShouldReturnTrue()
    {
        ScimPatcher<AppUser> sut = CreateSut;

        PatchCommand patchCommand = new PatchCommand(PatchOperation.Add, PathExpression.Root);

        IScimPatchOperationExecutor<AppUser> patchOp = new Mock<IScimPatchOperationExecutor<AppUser>>().Object;

        patchMap.Setup(map => map.TryGetPatchExecutor(
            patchCommand.Path,
            out patchOp
        )).Returns(true);

        bool result = sut.TryPatch(new AppUser(), patchCommand);

        result.Should().BeTrue();
    }

    [Fact]
    public void TryPatch_WhenCalledAndAPatchMapContainsKeyForPath_ShouldCallExecuteOnReturnedValue()
    {
        ScimPatcher<AppUser> sut = CreateSut;
        PatchCommand patchCommand = new PatchCommand(PatchOperation.Add, PathExpression.Root);
        Mock<IScimPatchOperationExecutor<AppUser>> mockOp = new Mock<IScimPatchOperationExecutor<AppUser>>();
        IScimPatchOperationExecutor<AppUser> patchOp = mockOp.Object;
        AppUser user = new AppUser();

        patchMap.Setup(map => map.TryGetPatchExecutor(
            patchCommand.Path,
            out patchOp
        )).Returns(true);

        sut.TryPatch(user, patchCommand);

        mockOp.Verify(mo => mo.Execute(user, patchCommand), Times.Once);
    }

    private ScimPatcher<AppUser> CreateSut => new([patchMap.Object]);
}