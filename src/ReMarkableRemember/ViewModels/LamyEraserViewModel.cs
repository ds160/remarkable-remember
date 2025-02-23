using System;
using System.Threading.Tasks;
using ReMarkableRemember.Services.TabletService;

namespace ReMarkableRemember.ViewModels;

public sealed partial class LamyEraserViewModel : DialogWindowModel
{
    private readonly ITabletService tabletService;

    internal LamyEraserViewModel(ITabletService tabletService) : base("Lamy Eraser Options", "Install", "Cancel")
    {
        this.tabletService = tabletService;

        this.LeftHanded = 0;
        this.Press = 0;
        this.Undo = 1;
    }

    public Int32 LeftHanded { get; set; }

    public Int32 Press { get; set; }

    public Int32 Undo { get; set; }

    protected override async Task<Boolean> OnClose()
    {
        await this.tabletService.InstallLamyEraser(this.Press != 0, this.Undo != 0, this.LeftHanded != 0).ConfigureAwait(true);

        return await base.OnClose().ConfigureAwait(true);
    }
}
