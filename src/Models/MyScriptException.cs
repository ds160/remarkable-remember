using System;

namespace ReMarkableRemember.Models;

public class MyScriptException : Exception
{
    public MyScriptException() { }

    public MyScriptException(String message) : base(message) { }

    public MyScriptException(String message, Exception innerException) : base(message, innerException) { }
}
