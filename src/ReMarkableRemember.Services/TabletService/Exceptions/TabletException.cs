using System;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.Services.TabletService.Exceptions;

public sealed class TabletException : Exception
{
    public TabletException() { }

    public TabletException(String message) : base(message) { }

    public TabletException(String message, Exception innerException) : base(message, innerException) { }

    public TabletException(TabletError error, String message) : base(message) { this.Error = error; }

    public TabletException(TabletError error, String message, Exception innerException) : base(message, innerException) { this.Error = error; }

    public TabletError Error { get; }
}
