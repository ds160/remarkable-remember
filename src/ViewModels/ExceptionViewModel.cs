using System;

namespace ReMarkableRemember.ViewModels;

public sealed class ExceptionViewModel : DialogWindowModel
{
    public ExceptionViewModel(String message) : base("Error", "OK")
    {
        this.Message = message;
    }

    public String Message { get; }
}
