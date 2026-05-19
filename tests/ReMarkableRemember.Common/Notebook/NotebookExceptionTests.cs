using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Common.Notebook.Exceptions;

namespace ReMarkableRemember.Common.Notebook.Tests;

[TestFixture]
public sealed class NotebookExceptionTests
{
    [Test]
    public void Constructor_WithMessage_PreservesMessage()
    {
        NotebookException exception = new NotebookException("boom");

        exception.Message.Should().Be("boom");
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public void Constructor_WithInnerException_PreservesBoth()
    {
        InvalidOperationException inner = new InvalidOperationException("inner");

        NotebookException exception = new NotebookException("outer", inner);

        exception.Message.Should().Be("outer");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
