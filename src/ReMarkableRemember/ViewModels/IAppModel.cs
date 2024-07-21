using System.Windows.Input;

namespace ReMarkableRemember.ViewModels;

public interface IAppModel
{
    ICommand CommandAbout { get; }
}