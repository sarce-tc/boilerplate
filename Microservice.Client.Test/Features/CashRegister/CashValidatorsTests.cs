using FluentAssertions;
using Microservice.Client.Features.CashRegister.Models;
using Microservice.Client.Features.CashRegister.Validators;
using Xunit;

namespace Microservice.Client.Test.Features.CashRegister;

public class CashValidatorsTests
{
    [Fact]
    public void OpenSession_requires_register_name()
    {
        var validator = new OpenSessionFormValidator();

        validator.Validate(new OpenSessionFormModel { RegisterName = "", OpeningBalance = 0 }).IsValid.Should().BeFalse();
        validator.Validate(new OpenSessionFormModel { RegisterName = "Caja 1", OpeningBalance = 0 }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void OpenSession_rejects_negative_opening_balance()
    {
        var validator = new OpenSessionFormValidator();

        validator.Validate(new OpenSessionFormModel { RegisterName = "Caja 1", OpeningBalance = -1 }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Movement_amount_must_be_positive()
    {
        var validator = new MovementFormValidator();

        validator.Validate(new MovementFormModel { Amount = 0 }).IsValid.Should().BeFalse();
        validator.Validate(new MovementFormModel { Amount = 100 }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Close_rejects_negative_declared_balance()
    {
        var validator = new CloseSessionFormValidator();

        validator.Validate(new CloseSessionFormModel { DeclaredBalance = -5 }).IsValid.Should().BeFalse();
        validator.Validate(new CloseSessionFormModel { DeclaredBalance = 0 }).IsValid.Should().BeTrue();
    }
}
