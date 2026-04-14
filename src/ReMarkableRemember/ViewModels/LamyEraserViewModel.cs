using System;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.ViewModels;

public sealed partial class LamyEraserViewModel : DialogWindowModel
{
    private readonly ServiceProvider services;

    internal LamyEraserViewModel(ServiceProvider services)
        : base(Language.Current.LamyEraserTitle, Language.Current.ButtonInstall, Language.Current.ButtonCancel)
    {
        this.services = services;

        this.LeftHanded = 0;
        this.Press = 0;
        this.Undo = 1;
    }

    public Int32 LeftHanded { get; set; }

    public Int32 Press { get; set; }

    public Int32 Undo { get; set; }

    protected override async Task<Boolean> OnClose()
    {
        await this.services.Tablet.InstallLamyEraser(this.Press != 0, this.Undo != 0, this.LeftHanded != 0).ConfigureAwait(true);

        return await base.OnClose().ConfigureAwait(true);
    }
}
