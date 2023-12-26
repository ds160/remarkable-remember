using System;

namespace ReMarkableRemember.ViewModels;

public sealed class ExceptionViewModel : DialogWindowModel
{
    public ExceptionViewModel(String message)
    {
        this.Message = message;
    }

    public String Message { get; }

    public override String Title { get { return "Error"; } }
}
