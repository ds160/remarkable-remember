using System.Windows.Input;

namespace ReMarkableRemember.ViewModels;

public interface IAppModel : ILocalizedViewModel
{
    ICommand CommandAbout { get; }

    ICommand CommandSettings { get; }
}
