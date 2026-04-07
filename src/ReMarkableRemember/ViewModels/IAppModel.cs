using System.ComponentModel;
using System.Windows.Input;
using ReMarkableRemember.Enumerations;

namespace ReMarkableRemember.ViewModels;

public interface IAppModel : ILocalizedViewModel, INotifyPropertyChanged
{
    ICommand CommandAbout { get; }

    ICommand CommandSettings { get; }

    ApplicationThemes ApplicationTheme { get; set; }
}
