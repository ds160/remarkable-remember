using System;

namespace ReMarkableRemember.Common.Notebook.Exceptions;

public sealed class NotebookException : Exception
{
    public NotebookException() { }

    public NotebookException(String message) : base(message) { }

    public NotebookException(String message, Exception innerException) : base(message, innerException) { }
}