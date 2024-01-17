using System;

namespace ReMarkableRemember.ViewModels;

public sealed partial class LamyEraserOptionsViewModel : DialogWindowModel
{
    internal LamyEraserOptionsViewModel() : base("Lamy Eraser Options", "Install", "Cancel")
    {
        this.LeftHanded = 0;
        this.Press = 0;
        this.Undo = 1;
    }

    public Int32 LeftHanded { get; set; }

    public Int32 Press { get; set; }

    public Int32 Undo { get; set; }
}
