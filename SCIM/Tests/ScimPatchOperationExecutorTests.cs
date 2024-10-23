using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Rsk.AspNetCore.Scim.Parsers;
using Rsk.AspNetCore.Scim.Stores;
using SimpleApp.Services;

namespace Tests;

public class TestUser
{
    public string Id { get; }
    public string Username { get; set; }

    public string foo;
}

public class ScimPatchOperationExecutorTests
{
    private MemberExpression memberExpression;
    private LiteralConverter literalConverter = null;

    [Fact]
    public void ctor_WhenCalledWithMemberExpression_WhereMemberIsNotProperty_ShouldThrowException()
    {
        Expression<Func<TestUser, object>> exp = appUser => appUser.foo;
        memberExpression = exp.Body as MemberExpression;

        Action act = () => { _ = CreateSut; };

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ctor_WhenCalledWithMemberExpression_WhereMemberHasNoAvailableSetter_ShouldThrowException()
    {
        Expression<Func<TestUser, string>> exp = appUser => appUser.Id;
        memberExpression = exp.Body as MemberExpression;

        Action act = () => { _ = CreateSut; };

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Execute_WhenConverterIsNull_ShouldSetValue()
    {
        Expression<Func<TestUser, string>> exp = appUser => appUser.Username;
        memberExpression = exp.Body as MemberExpression;

        string value = "value";
        TestUser target = new TestUser();
        PatchCommand command = new PatchCommand(PatchOperation.Add, new PathExpression(), value);

        ScimPatchOperationExecutor<TestUser> sut = CreateSut;

        sut.Execute(target, command);

        target.Username.Should().Be(value);
    }

    [Fact]
    public void Execute_WhenCalledAndConverterIsNotNull_ShouldConvertAndSetValue()
    {
        Expression<Func<TestUser, string>> exp = appUser => appUser.Username;
        memberExpression = exp.Body as MemberExpression;

        string value = "value";
        TestUser target = new TestUser();
        PatchCommand command = new PatchCommand(PatchOperation.Add, new PathExpression(), value);

        Mock<LiteralConverter> converter = new Mock<LiteralConverter>();
        converter.Setup(x => x(value)).Returns("converted value");
        literalConverter = converter.Object;

        ScimPatchOperationExecutor<TestUser> sut = CreateSut;

        sut.Execute(target, command);

        target.Username.Should().Be("converted value");
    }

    private ScimPatchOperationExecutor<TestUser> CreateSut => new (memberExpression, literalConverter);
}