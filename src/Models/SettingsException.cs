using System;

namespace ReMarkableRemember.Models;

public class SettingsException : Exception
{
    public SettingsException() { }

    public SettingsException(String message) : base(message) { }

    public SettingsException(String message, Exception innerException) : base(message, innerException) { }
}
