using System;

namespace ReMarkableRemember.Models;

public sealed class TabletException : Exception
{

    public TabletException() { }

    public TabletException(String message) : base(message) { }

    public TabletException(String message, Exception innerException) : base(message, innerException) { }

    public TabletException(TabletConnectionError error, String message, Exception innerException) : base(message, innerException) { this.Error = error; }

    public TabletConnectionError Error { get; }
}
