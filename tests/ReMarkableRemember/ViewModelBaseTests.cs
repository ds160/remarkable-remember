using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class ViewModelBaseTests
{
    private sealed class TestViewModel : ViewModelBase
    {
        public void TriggerError(String property, String message)
        {
            this.AddError(property, message);
        }

        public void TriggerClear(String? property = null)
        {
            this.ClearErrors(property);
        }
    }

    [Test]
    public void HasErrors_Initially_False()
    {
        TestViewModel vm = new TestViewModel();

        vm.HasErrors.Should().BeFalse();
    }

    [Test]
    public void AddError_SetsHasErrorsAndRaisesEvent()
    {
        TestViewModel vm = new TestViewModel();
        DataErrorsChangedEventArgs? captured = null;
        vm.ErrorsChanged += (_, args) => captured = args;

        vm.TriggerError("Foo", "Foo is wrong");

        vm.HasErrors.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.PropertyName.Should().Be("Foo");
    }

    [Test]
    public void GetErrors_ReturnsErrorsForPropertyOnly()
    {
        TestViewModel vm = new TestViewModel();
        vm.TriggerError("A", "errA1");
        vm.TriggerError("A", "errA2");
        vm.TriggerError("B", "errB");

        ValidationResult[] forA = vm.GetErrors("A").Cast<ValidationResult>().ToArray();

        forA.Should().HaveCount(2);
        forA.Select(r => r.ErrorMessage).Should().Contain("errA1");
    }

    [Test]
    public void GetErrors_NoProperty_ReturnsAllErrors()
    {
        TestViewModel vm = new TestViewModel();
        vm.TriggerError("A", "errA");
        vm.TriggerError("B", "errB");

        IEnumerable all = vm.GetErrors(null);

        all.Cast<ValidationResult>().Should().HaveCount(2);
    }

    [Test]
    public void GetErrors_UnknownProperty_ReturnsEmpty()
    {
        TestViewModel vm = new TestViewModel();

        IEnumerable errors = vm.GetErrors("Missing");

        errors.Cast<ValidationResult>().Should().BeEmpty();
    }

    [Test]
    public void ClearErrors_NoProperty_RemovesAll()
    {
        TestViewModel vm = new TestViewModel();
        vm.TriggerError("A", "a");
        vm.TriggerError("B", "b");

        vm.TriggerClear();

        vm.HasErrors.Should().BeFalse();
    }

    [Test]
    public void ClearErrors_WithProperty_OnlyRemovesThatProperty()
    {
        TestViewModel vm = new TestViewModel();
        vm.TriggerError("A", "a");
        vm.TriggerError("B", "b");

        vm.TriggerClear("A");

        vm.HasErrors.Should().BeTrue();
        vm.GetErrors("A").Cast<ValidationResult>().Should().BeEmpty();
        vm.GetErrors("B").Cast<ValidationResult>().Should().NotBeEmpty();
    }
}
