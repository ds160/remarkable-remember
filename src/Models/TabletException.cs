using System;

namespace ReMarkableRemember.Models;

public sealed class TabletException : Exception
{
    public TabletException()
    {
    }

    public TabletException(String message) : base(message)
    {
    }

    public TabletException(String message, Exception innerException) : base(message, innerException)
    {
    }
}
